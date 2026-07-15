namespace OmniBlox.Application.Features.Products.DTOs;

public record LowStockDetailItem
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public string Category { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public int AlertQuantity { get; init; }
}

public record LowStockDetailsResponse
{
    public List<LowStockDetailItem> Items { get; init; } = [];
    public int Total { get; init; }
    public int Pages { get; init; }
}
