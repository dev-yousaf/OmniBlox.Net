namespace OmniBlox.Api.Controllers.Requests;

public record UpdateQuotationStatusRequest
{
    public string Status { get; init; } = string.Empty;
}

public record ConvertQuotationRequest
{
    public Guid WarehouseId { get; init; }
    public DateTime SaleDate { get; init; }
    public DateTime DueDate { get; init; }
    public string? Status { get; init; }
    public string? PaymentStatus { get; init; }
    public string? PaymentMethod { get; init; }
    public string? Notes { get; init; }
    public string? ShippingAddress { get; init; }
}
