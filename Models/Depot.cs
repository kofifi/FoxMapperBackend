namespace FoxMapperBackend.Models;

public class Depot
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? AddressLine { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }

    // Współrzędne magazynu / punktu startowego
    public double Lat { get; set; }
    public double Lng { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;

    // Nawigacja
    public ICollection<DeliveryRun> DeliveryRuns { get; set; } = new List<DeliveryRun>();
}