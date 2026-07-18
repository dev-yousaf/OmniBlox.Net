using OmniBlox.Domain.Entities;

namespace OmniBlox.Application.Features.Products.DTOs;

public record CreatedByInfo
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Image { get; init; }
}

public record ProductDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string SKU { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string? SubCategory { get; init; }
    public string? Brand { get; init; }
    public string Unit { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public decimal SalePrice { get; init; }
    public decimal CostPrice { get; init; }
    public int Stock { get; init; }
    public int ReorderLevel { get; init; }
    public Guid? WarehouseId { get; init; }
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
    public CreatedByInfo? CreatedBy { get; init; }

    public static ProductDto FromEntity(Product product, Guid? warehouseId = null) => new()
    {
        Id = product.Id,
        Name = product.Name,
        SKU = product.SKU,
        Description = product.Description,
        Type = product.Type.ToString(),
        Category = product.Category,
        SubCategory = product.SubCategory,
        Brand = product.Brand,
        Unit = product.Unit,
        ImageUrl = product.ImageUrl,
        SalePrice = product.SalePrice,
        CostPrice = product.CostPrice,
        Stock = product.Stock,
        ReorderLevel = product.ReorderLevel,
        WarehouseId = warehouseId,
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
        CreatedBy = product.CreatedByUser is not null
            ? new CreatedByInfo { Id = product.CreatedByUser.Id.ToString(), Name = product.CreatedByUser.Name }
            : null,
    };
}

public record ProductListResponse
{
    public List<ProductDto> Products { get; init; } = [];
    public int Total { get; init; }
    public int Pages { get; init; }
    public int Page { get; init; }
    public int Limit { get; init; }
}


