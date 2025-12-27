using System.Text.Json.Serialization;

namespace FoxMapperBackend.Models.Routing;

// /table/v1/driving/... ?annotations=distance
public class OsrmTableResponse
{
    [JsonPropertyName("distances")]
    public double[][]? Distances { get; set; }
}

// /route/v1/driving/... ?overview=full&geometries=geojson
public class OsrmRouteResponse
{
    [JsonPropertyName("routes")]
    public List<OsrmRoute>? Routes { get; set; }
}

public class OsrmRoute
{
    [JsonPropertyName("geometry")]
    public OsrmGeometry? Geometry { get; set; }

    [JsonPropertyName("distance")]
    public double Distance { get; set; }

    [JsonPropertyName("duration")]
    public double Duration { get; set; }
}

public class OsrmGeometry
{
    // GeoJSON LineString
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    // [[lng, lat], [lng, lat], ...]
    [JsonPropertyName("coordinates")]
    public List<List<double>>? Coordinates { get; set; }
}