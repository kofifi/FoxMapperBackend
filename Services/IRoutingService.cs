using FoxMapperBackend.Models.Routing;

namespace FoxMapperBackend.Services;

public interface IRoutingService
{
    /// <summary>
    /// Zwraca kolejność odwiedzenia punktów (TSP heurystyka)
    /// na podstawie dystansu po drogach (OSRM /table).
    /// Zwraca listę indeksów, np. [0, 3, 1, 2].
    /// </summary>
    Task<IReadOnlyList<int>> GetDrivingOrderAsync(IReadOnlyList<RoutingPoint> points);

    /// <summary>
    /// Zwraca geometrię trasy (lista punktów po drogach) dla zadanej kolejności.
    /// (OSRM /route z geometries=geojson)
    /// </summary>
    Task<IReadOnlyList<RoutingPoint>> GetRouteGeometryAsync(IReadOnlyList<RoutingPoint> orderedPoints);
}