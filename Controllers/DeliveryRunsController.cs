using FoxMapperBackend.Data;
using FoxMapperBackend.Models;
using FoxMapperBackend.Models.Requests;
using FoxMapperBackend.Models.Routing;
using FoxMapperBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoxMapperBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeliveryRunsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IRoutingService _routingService;
    private readonly ICourierLocationStore _locationStore;
    private readonly Random _random = new();

    public DeliveryRunsController(AppDbContext db, IRoutingService routingService, ICourierLocationStore locationStore)
    {
        _db = db;
        _routingService = routingService;
        _locationStore = locationStore;
    }

    private sealed record DeliveryRunPreset(
        int Id,
        string Code,
        string Area,
        int PackageCount,
        int EstimatedMinutes,
        double DistanceKm,
        bool ReturnToDepot = false);

    private static readonly DeliveryRunPreset[] Presets =
    [
        new(1, "Ekspres (5 paczek)", "Śródmieście", 5, 25, 8.0),
        new(2, "Szybki kurs (10 paczek)", "Śródmieście", 10, 45, 15.0),
        new(3, "Standard (20 paczek)", "Miasto", 20, 90, 35.0),
        new(4, "Duży kurs (40 paczek)", "Cały rejon", 40, 180, 80.0),
        new(5, "Mega (60 paczek)", "Cały rejon", 60, 260, 120.0),
        new(6, "Z powrotem do magazynu (20 paczek)", "Miasto", 20, 105, 42.0, ReturnToDepot: true),
    ];

    // GET: api/deliveryruns/available
    //
    // Ekran wyboru kursu w froncie (RunSelectionScreen) woła ten endpoint.
    // Na razie zwracamy 3 "presety" na sztywno – idealne do testów UI.
    [HttpGet("available")]
    public ActionResult<IEnumerable<object>> GetAvailableRuns()
    {
        return Ok(Presets.Select(p => new
        {
            p.Id,
            p.Code,
            p.Area,
            p.PackageCount,
            p.EstimatedMinutes,
            p.DistanceKm
        }));
    }

    // POST: api/deliveryruns/{presetId}/start
    //
    // To jest endpoint, który woła front:
    // POST /api/deliveryruns/2/start
    //
    // Na podstawie presetId ustawiamy PackageCount, a CourierId / DepotId na razie na sztywno (1,1).
    // Następnie przekazujemy to do istniejącej logiki StartRun(StartRunRequest).
    [HttpPost("{presetId:int}/start")]
    public Task<IActionResult> StartRunFromPreset(int presetId)
    {
        var preset = Presets.FirstOrDefault(p => p.Id == presetId) ?? Presets[1];

        // TODO: docelowo CourierId i DepotId bierzemy z zalogowanego usera / konfiguracji
        var request = new StartRunRequest
        {
            CourierId = 1,
            DepotId = 1,
            PackageCount = preset.PackageCount,
            ReturnToDepot = preset.ReturnToDepot
        };

        // Reużywamy istniejącej logiki startu przejazdu
        return StartRun(request);
    }

    // POST: api/deliveryruns/start
    //
    // Główna logika tworzenia przejazdu, losowania paczek
    // oraz wyznaczania trasy z użyciem OSRM.
    [HttpPost("start")]
    public async Task<IActionResult> StartRun([FromBody] StartRunRequest request)
    {
        var courier = await _db.Couriers.FindAsync(request.CourierId);
        if (courier == null)
        {
            return NotFound($"Courier with id {request.CourierId} not found.");
        }

        var depot = await _db.Depots.FindAsync(request.DepotId);
        if (depot == null)
        {
            return NotFound($"Depot with id {request.DepotId} not found.");
        }

        if (request.PackageCount <= 0)
        {
            request.PackageCount = 10;
        }

        // 1) Tworzymy DeliveryRun
        var run = new DeliveryRun
        {
            CourierId = courier.Id,
            DepotId = depot.Id,
            Status = DeliveryRunStatus.InProgress,
            StartTimeUtc = DateTime.UtcNow,
            LastUpdatedUtc = DateTime.UtcNow
        };

        _db.DeliveryRuns.Add(run);
        await _db.SaveChangesAsync(); // potrzebne, żeby mieć run.Id

        // 2) Generujemy losowe paczki w okolicy magazynu
        var packages = new List<Package>();

        for (int i = 0; i < request.PackageCount; i++)
        {
            // mały offset współrzędnych wokół magazynu (~kilka km)
            var latOffset = (_random.NextDouble() - 0.5) * 0.1; // ok. +/- 0.05° = kilka km
            var lngOffset = (_random.NextDouble() - 0.5) * 0.1;

            var package = new Package
            {
                DeliveryRunId = run.Id,
                ExternalCode = $"PKG-{run.Id}-{i + 1}",
                RecipientName = $"Recipient {i + 1}",
                AddressLine = null,
                City = null,
                PostalCode = null,
                Lat = depot.Lat + latOffset,
                Lng = depot.Lng + lngOffset,
                Status = PackageStatus.Assigned,
                CreatedAtUtc = DateTime.UtcNow,
                LastUpdatedUtc = DateTime.UtcNow
            };

            packages.Add(package);
        }

        _db.Packages.AddRange(packages);
        await _db.SaveChangesAsync();

        // 3) Przygotowujemy punkty do OSRM: 0 = depot, 1..N = paczki
        var osrmPoints = new List<RoutingPoint>
        {
            new RoutingPoint(depot.Lat, depot.Lng)
        };
        osrmPoints.AddRange(packages.Select(p => new RoutingPoint(p.Lat, p.Lng)));

        // 3a) Pobieramy kolejność odwiedzania punktów po drogach
        var order = await _routingService.GetDrivingOrderAsync(osrmPoints);
        // order to np. [0, 3, 1, 2, ...]

        // 3b) Tworzymy RouteStops na podstawie kolejności
        var routeStops = new List<RouteStop>();
        var orderIndex = 0;

        foreach (var idx in order)
        {
            if (idx == 0)
            {
                // Depot jako pierwszy punkt
                routeStops.Add(new RouteStop
                {
                    DeliveryRunId = run.Id,
                    OrderIndex = orderIndex++,
                    Type = RouteStopType.Depot,
                    PackageId = null,
                    Lat = depot.Lat,
                    Lng = depot.Lng,
                    CreatedAtUtc = DateTime.UtcNow,
                    LastUpdatedUtc = DateTime.UtcNow
                });
            }
            else
            {
                // idx > 0 -> paczka o indeksie idx - 1 w liście packages
                var package = packages[idx - 1];

                routeStops.Add(new RouteStop
                {
                    DeliveryRunId = run.Id,
                    OrderIndex = orderIndex++,
                    Type = RouteStopType.Package,
                    PackageId = package.Id,
                    Lat = package.Lat,
                    Lng = package.Lng,
                    CreatedAtUtc = DateTime.UtcNow,
                    LastUpdatedUtc = DateTime.UtcNow
                });
            }
        }

        if (request.ReturnToDepot)
        {
            routeStops.Add(new RouteStop
            {
                DeliveryRunId = run.Id,
                OrderIndex = orderIndex++,
                Type = RouteStopType.Depot,
                PackageId = null,
                Lat = depot.Lat,
                Lng = depot.Lng,
                CreatedAtUtc = DateTime.UtcNow,
                LastUpdatedUtc = DateTime.UtcNow
            });
        }

        _db.RouteStops.AddRange(routeStops);

        // 3c) Dodatkowo pobieramy pełną geometrię trasy po drogach (polyline)
        var courierLocation = _locationStore.Get(courier.Id);
        var orderedPointsForGeometry = order.Select(i => osrmPoints[i]).ToList();
        if (courierLocation != null)
        {
            orderedPointsForGeometry.Insert(0, new RoutingPoint(courierLocation.Lat, courierLocation.Lng));
        }

        var routeGeometry = await _routingService.GetRouteGeometryAsync(orderedPointsForGeometry);

        run.LastUpdatedUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // 4) Ładujemy run z nawigacjami do odpowiedzi
        var result = await _db.DeliveryRuns
            .Include(r => r.Packages)
            .Include(r => r.RouteStops)
            .AsNoTracking()
            .FirstAsync(r => r.Id == run.Id);

        courierLocation = _locationStore.Get(result.CourierId);

        // 5) Składamy odpowiedź – dodajemy RouteGeometry
        var response = new
        {
            result.Id,
            result.CourierId,
            result.DepotId,
            Status = result.Status.ToString(),
            result.StartTimeUtc,
            result.EndTimeUtc,
            CourierLocation = courierLocation,

            Packages = result.Packages.Select(p => new
            {
                p.Id,
                p.ExternalCode,
                p.RecipientName,
                p.Lat,
                p.Lng,
                Status = p.Status.ToString()
            }),

            RouteStops = result.RouteStops
                .OrderBy(rs => rs.OrderIndex)
                .Select(rs => new
                {
                    rs.OrderIndex,
                    Type = rs.Type.ToString(),
                    rs.PackageId,
                    rs.Lat,
                    rs.Lng
                }),

            RouteGeometry = routeGeometry.Select(pt => new
            {
                pt.Lat,
                pt.Lng
            })
        };

        return Ok(response);
    }

    // GET: api/deliveryruns/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetRun(int id)
    {
        var run = await _db.DeliveryRuns
            .Include(r => r.Packages)
            .Include(r => r.RouteStops)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (run == null)
            return NotFound();

        // 1) Układamy punkty dla OSRM w kolejności RouteStops
        var orderedStops = run.RouteStops
            .OrderBy(rs => rs.OrderIndex)
            .ToList();

        var osrmPoints = orderedStops
            .Select(rs => new RoutingPoint(rs.Lat, rs.Lng))
            .ToList();

        IReadOnlyList<RoutingPoint> routeGeometry;
        var courierLocation = _locationStore.Get(run.CourierId);
        try
        {
            if (courierLocation != null)
            {
                osrmPoints.Insert(0, new RoutingPoint(courierLocation.Lat, courierLocation.Lng));
            }

            // 2) Pobieramy geometrię trasy po drogach z OSRM
            routeGeometry = await _routingService.GetRouteGeometryAsync(osrmPoints);
        }
        catch
        {
            // fallback – jakby OSRM padł, użyjemy zwykłej łamanej po RouteStops
            routeGeometry = osrmPoints;
        }

        // 3) Budujemy odpowiedź z RouteGeometry
        var response = new
        {
            run.Id,
            run.CourierId,
            run.DepotId,
            Status = run.Status.ToString(),
            run.StartTimeUtc,
            run.EndTimeUtc,
            run.LastUpdatedUtc,
            CourierLocation = courierLocation,

            Packages = run.Packages.Select(p => new
            {
                p.Id,
                p.ExternalCode,
                p.RecipientName,
                p.Lat,
                p.Lng,
                Status = p.Status.ToString()
            }),

            RouteStops = orderedStops.Select(rs => new
            {
                rs.OrderIndex,
                Type = rs.Type.ToString(),
                rs.PackageId,
                rs.Lat,
                rs.Lng
            }),

            RouteGeometry = routeGeometry.Select(pt => new
            {
                pt.Lat,
                pt.Lng
            })
        };

        return Ok(response);
    }


    // ====== Pomocnicze (na razie nieużywane, ale zostawiam) ======

    private List<RouteStop> BuildRouteStops(int runId, Depot depot, List<Package> packages)
    {
        var routeStops = new List<RouteStop>();
        var orderIndex = 0;

        // 1) Start – magazyn
        routeStops.Add(new RouteStop
        {
            DeliveryRunId = runId,
            OrderIndex = orderIndex++,
            Type = RouteStopType.Depot,
            PackageId = null,
            Lat = depot.Lat,
            Lng = depot.Lng,
            CreatedAtUtc = DateTime.UtcNow,
            LastUpdatedUtc = DateTime.UtcNow
        });

        // 2) Najprostsza heurystyka: "nearest neighbour"
        var remaining = new List<Package>(packages);
        var currentLat = depot.Lat;
        var currentLng = depot.Lng;

        while (remaining.Count > 0)
        {
            var next = remaining
                .OrderBy(p => Distance(currentLat, currentLng, p.Lat, p.Lng))
                .First();

            routeStops.Add(new RouteStop
            {
                DeliveryRunId = runId,
                OrderIndex = orderIndex++,
                Type = RouteStopType.Package,
                PackageId = next.Id,
                Lat = next.Lat,
                Lng = next.Lng,
                CreatedAtUtc = DateTime.UtcNow,
                LastUpdatedUtc = DateTime.UtcNow
            });

            currentLat = next.Lat;
            currentLng = next.Lng;

            remaining.Remove(next);
        }

        return routeStops;
    }

    // Prosty dystans euklidesowy "po stopniach"
    private static double Distance(double lat1, double lng1, double lat2, double lng2)
    {
        var dLat = lat2 - lat1;
        var dLng = lng2 - lng1;
        return Math.Sqrt(dLat * dLat + dLng * dLng);
    }
}
