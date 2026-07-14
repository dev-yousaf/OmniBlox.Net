using OmniBlox.Domain.Enums;
using OmniBlox.Domain.Interfaces;

namespace OmniBlox.Domain.Entities;

public class Product : BaseEntity, ITenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProductType Type { get; set; } = ProductType.STANDARD;
    public string Category { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }

    public decimal SalePrice { get; set; }
    public decimal CostPrice { get; set; }
    public int StockQuantity { get; set; }
    public int ReorderLevel { get; set; }

    public ProductStatus Status { get; set; } = ProductStatus.ACTIVE;

    public string? BarcodeSymbology { get; set; }
    public decimal? TaxRate { get; set; }
    public int? AlertQuantity { get; set; }
    public string? ItemCode { get; set; }
    public string? Manufacturer { get; set; }
    public string? Warranty { get; set; }
    public DateTime? ManufacturedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
}
