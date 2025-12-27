namespace FoxMapperBackend.Models;

public class Courier
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty; // np. numer pracownika
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;

    // Nawigacja
    public ICollection<DeliveryRun> DeliveryRuns { get; set; } = new List<DeliveryRun>();
}