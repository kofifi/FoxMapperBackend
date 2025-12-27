namespace FoxMapperBackend.Models;

public class RouteStop
{
    public int Id { get; set; }

    public int DeliveryRunId { get; set; }
    public DeliveryRun DeliveryRun { get; set; } = null!;

    public int OrderIndex { get; set; } // 0 = depot, 1..N = kolejne przystanki

    public RouteStopType Type { get; set; } = RouteStopType.Package;

    // Gdy przystanek dotyczy konkretnej paczki
    public int? PackageId { get; set; }
    public Package? Package { get; set; }

    // Współrzędne przystanku (mogą być takie same jak w paczce)
    public double Lat { get; set; }
    public double Lng { get; set; }

    // Opcjonalnie planowany czas przyjazdu
    public DateTime? PlannedArrivalUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
}