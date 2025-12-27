namespace FoxMapperBackend.Models.DTOs;

public class DeliveryRunSummaryDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;   // np. "Run #1234"
    public string Area { get; set; } = string.Empty;   // np. "Kraków – Krowodrza"
    public int PackageCount { get; set; }              // ile paczek
    public int? EstimatedMinutes { get; set; }         // szacowany czas
    public double? DistanceKm { get; set; }            // szacowany dystans
}