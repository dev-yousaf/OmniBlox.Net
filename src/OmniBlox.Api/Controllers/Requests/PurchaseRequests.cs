namespace OmniBlox.Api.Controllers.Requests;

public record ReceivePurchaseRequest
{
    public Guid WarehouseId { get; init; }
}
