namespace FoxMapperBackend.Models.DTOs;

public class CourierLocationDto
{
    public double Lat { get; set; }
    public double Lng { get; set; }

    public double? AccuracyM { get; set; }
    public double? HeadingDeg { get; set; }
    public double? SpeedMps { get; set; }

    public DateTime ObservedAtUtc { get; set; } = DateTime.UtcNow;
}

