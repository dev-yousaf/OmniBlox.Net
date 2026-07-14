using OmniBlox.Domain.Entities;

namespace OmniBlox.Application.Features.Products.DTOs;

public record ProductDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string SKU { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string? Brand { get; init; }
    public string Unit { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public decimal SalePrice { get; init; }
    public decimal CostPrice { get; init; }
    public int StockQuantity { get; init; }
    public int ReorderLevel { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? BarcodeSymbology { get; init; }
    public decimal? TaxRate { get; init; }
    public int? AlertQuantity { get; init; }
    public string? ItemCode { get; init; }
    public string? Manufacturer { get; init; }
    public string? Warranty { get; init; }
    public DateTime? ManufacturedDate { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }

    public static ProductDto FromEntity(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        SKU = product.SKU,
        Description = product.Description,
        Type = product.Type.ToString(),
        Category = product.Category,
        Brand = product.Brand,
        Unit = product.Unit,
        ImageUrl = product.ImageUrl,
        SalePrice = product.SalePrice,
        CostPrice = product.CostPrice,
        StockQuantity = product.StockQuantity,
        ReorderLevel = product.ReorderLevel,
        Status = product.Status.ToString(),
        BarcodeSymbology = product.BarcodeSymbology,
        TaxRate = product.TaxRate,
        AlertQuantity = product.AlertQuantity,
        ItemCode = product.ItemCode,
        Manufacturer = product.Manufacturer,
        Warranty = product.Warranty,
        ManufacturedDate = product.ManufacturedDate,
        ExpiryDate = product.ExpiryDate,
        CreatedAt = product.CreatedAt,
        UpdatedAt = product.UpdatedAt,
    };
}

public record ProductListResponse
{
    public List<ProductDto> Items { get; init; } = [];
    public int Total { get; init; }
    public int Page { get; init; }
    public int Limit { get; init; }
}

public record CreateProductRequest
{
    public string Name { get; init; } = string.Empty;
    public string SKU { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Type { get; init; }
    public string Category { get; init; } = string.Empty;
    public string? Brand { get; init; }
    public string Unit { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public decimal SalePrice { get; init; }
    public decimal CostPrice { get; init; }
    public int StockQuantity { get; init; }
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
}

public record UpdateProductRequest
{
    public string? Name { get; init; }
    public string? SKU { get; init; }
    public string? Description { get; init; }
    public string? Type { get; init; }
    public string? Category { get; init; }
    public string? Brand { get; init; }
    public string? Unit { get; init; }
    public string? ImageUrl { get; init; }
    public decimal? SalePrice { get; init; }
    public decimal? CostPrice { get; init; }
    public int? StockQuantity { get; init; }
    public int? ReorderLevel { get; init; }
    public string? Status { get; init; }
    public string? BarcodeSymbology { get; init; }
    public decimal? TaxRate { get; init; }
    public int? AlertQuantity { get; init; }
    public string? ItemCode { get; init; }
    public string? Manufacturer { get; init; }
    public string? Warranty { get; init; }
    public DateTime? ManufacturedDate { get; init; }
    public DateTime? ExpiryDate { get; init; }
}
