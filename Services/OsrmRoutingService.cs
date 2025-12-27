using System.Globalization;
using FoxMapperBackend.Models.Routing;

namespace FoxMapperBackend.Services;

public class OsrmRoutingService : IRoutingService
{
    private readonly HttpClient _httpClient;

    public OsrmRoutingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<int>> GetDrivingOrderAsync(IReadOnlyList<RoutingPoint> points)
    {
        if (points.Count == 0)
            return Array.Empty<int>();

        if (points.Count == 1)
            return new[] { 0 };

        // 1) Budujemy ciąg współrzędnych w formacie OSRM: lng,lat;lng,lat;...
        var coordsParam = string.Join(";", points.Select(p =>
            $"{p.Lng.ToString(CultureInfo.InvariantCulture)},{p.Lat.ToString(CultureInfo.InvariantCulture)}"));

        var url = $"table/v1/driving/{coordsParam}?annotations=distance";

        var response = await _httpClient.GetFromJsonAsync<OsrmTableResponse>(url);
        if (response?.Distances == null)
            throw new InvalidOperationException("OSRM /table response has no distances.");

        var distances = response.Distances;
        var n = points.Count;

        // 2) Heurystyka "nearest neighbour" po macierzy distances
        var visited = new bool[n];
        var order = new List<int>(n);

        int current = 0; // startujemy od pierwszego punktu (index 0 = depo)
        order.Add(current);
        visited[current] = true;

        for (int step = 1; step < n; step++)
        {
            int bestIndex = -1;
            double bestDist = double.MaxValue;

            for (int cand = 0; cand < n; cand++)
            {
                if (visited[cand])
                    continue;

                var d = distances[current][cand];
                if (d < bestDist)
                {
                    bestDist = d;
                    bestIndex = cand;
                }
            }

            if (bestIndex == -1)
                break;

            order.Add(bestIndex);
            visited[bestIndex] = true;
            current = bestIndex;
        }

        return order;
    }

    public async Task<IReadOnlyList<RoutingPoint>> GetRouteGeometryAsync(IReadOnlyList<RoutingPoint> orderedPoints)
    {
        if (orderedPoints.Count < 2)
            return orderedPoints.ToList();

        var coordsParam = string.Join(";", orderedPoints.Select(p =>
            $"{p.Lng.ToString(CultureInfo.InvariantCulture)},{p.Lat.ToString(CultureInfo.InvariantCulture)}"));

        var url = $"route/v1/driving/{coordsParam}?overview=full&geometries=geojson";

        var response = await _httpClient.GetFromJsonAsync<OsrmRouteResponse>(url);
        var route = response?.Routes?.FirstOrDefault();
        var coords = route?.Geometry?.Coordinates;

        if (coords == null || coords.Count == 0)
            return orderedPoints.ToList(); // fallback

        var result = coords
            .Select(c => new RoutingPoint(c[1], c[0])) // [lng, lat] -> (lat, lng)
            .ToList();


        return result;
    }
}
