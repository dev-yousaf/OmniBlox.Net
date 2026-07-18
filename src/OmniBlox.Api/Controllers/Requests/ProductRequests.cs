namespace OmniBlox.Api.Controllers.Requests;

public record UpdateStockRequest
{
    public int Quantity { get; init; }
    public string Operation { get; init; } = "add";
}
