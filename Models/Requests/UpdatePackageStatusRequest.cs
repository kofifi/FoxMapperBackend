namespace FoxMapperBackend.Models.Requests;

public class UpdatePackageStatusRequest
{
    public PackageStatus Status { get; set; }

    // Opcjonalnie – czas aktualizacji po stronie klienta (do offline sync)
    public DateTime? UpdatedAtClientUtc { get; set; }
}