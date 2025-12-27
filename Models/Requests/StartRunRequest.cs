namespace FoxMapperBackend.Models.Requests;

public class StartRunRequest
{
    public int CourierId { get; set; }
    public int DepotId { get; set; }
    public int PackageCount { get; set; } = 10;

    // Jeśli true, to trasa kończy się powrotem do magazynu.
    public bool ReturnToDepot { get; set; } = false;
}
