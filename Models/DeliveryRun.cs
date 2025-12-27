namespace FoxMapperBackend.Models;

public class DeliveryRun
{
    public int Id { get; set; }

    public int CourierId { get; set; }
    public Courier Courier { get; set; } = null!;

    public int DepotId { get; set; }
    public Depot Depot { get; set; } = null!;

    public DeliveryRunStatus Status { get; set; } = DeliveryRunStatus.Planned;

    public DateTime StartTimeUtc { get; set; } = DateTime.UtcNow;
    public DateTime? EndTimeUtc { get; set; }

    // pod offline / synchronizację
    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;

    // Nawigacja
    public ICollection<Package> Packages { get; set; } = new List<Package>();
    public ICollection<RouteStop> RouteStops { get; set; } = new List<RouteStop>();
}