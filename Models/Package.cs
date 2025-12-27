namespace FoxMapperBackend.Models;

public class Package
{
    public int Id { get; set; }

    public int DeliveryRunId { get; set; }
    public DeliveryRun DeliveryRun { get; set; } = null!;

    public string ExternalCode { get; set; } = string.Empty; // numer przesyłki / etykiety
    public string RecipientName { get; set; } = string.Empty;

    public string? AddressLine { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }

    public double Lat { get; set; }
    public double Lng { get; set; }

    public PackageStatus Status { get; set; } = PackageStatus.Assigned;

    public DateTime? DeliveredAtUtc { get; set; }

    // do offline sync
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;

    // Nawigacja
    public ICollection<RouteStop> RouteStops { get; set; } = new List<RouteStop>();
}