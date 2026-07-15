namespace OmniBlox.Application.Features.Products.DTOs;

public record CreateProductRequest
{
    public string Name { get; init; } = string.Empty;
    public string SKU { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Type { get; init; }
    public string Category { get; init; } = string.Empty;
    public string? SubCategory { get; init; }
    public string? Brand { get; init; }
    public string Unit { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public decimal SalePrice { get; init; }
    public decimal CostPrice { get; init; }
    public int Stock { get; init; }
    public int ReorderLevel { get; init; }
    public string? Status { get; init; }
    public string? BarcodeSymbology { get; init; }
    public decimal? TaxRate { get; init; }
    public int? AlertQuantity { get; init; }
    public string? ItemCode { get; init; }
    public string? Manufacturer { get; init; }
    public string? Warranty { get; init; }
    public DateTime? ManufacturedDate { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public Guid? WarehouseId { get; init; }
}
