namespace OmniBlox.Api.Controllers.Requests;

public record UpdateInventoryRequest
{
    public int Quantity { get; init; }
    public string? Notes { get; init; }
}
