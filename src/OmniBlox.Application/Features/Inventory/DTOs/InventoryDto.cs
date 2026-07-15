using OmniBlox.Domain.Entities;
using Inv = OmniBlox.Domain.Entities.Inventory;

namespace OmniBlox.Application.Features.Inventory.DTOs;

public record InventoryDto
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string ProductSku { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public Guid WarehouseId { get; init; }
    public string WarehouseName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal SalePrice { get; init; }
    public decimal CostPrice { get; init; }
    public int ReorderLevel { get; init; }
    public decimal StockValue { get; init; }
    public string Status { get; init; } = "in_stock";
    public string Category { get; init; } = string.Empty;
    public string? Brand { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }

    public static InventoryDto FromEntity(Inv entity)
    {
        var qty = entity.Quantity;
        var reorder = entity.Product?.ReorderLevel ?? 0;
        var costPrice = entity.Product?.CostPrice ?? 0;
        var status = qty <= 0 ? "out_of_stock" : qty <= reorder ? "low_stock" : "in_stock";

        return new()
        {
            ProductId = entity.ProductId,
            ProductName = entity.Product?.Name ?? "",
            ProductSku = entity.Product?.SKU ?? "",
            ImageUrl = entity.Product?.ImageUrl,
            WarehouseId = entity.WarehouseId,
            WarehouseName = entity.Warehouse?.Name ?? "",
            Quantity = qty,
            SalePrice = entity.Product?.SalePrice ?? 0,
            CostPrice = costPrice,
            ReorderLevel = reorder,
            StockValue = qty * costPrice,
            Status = status,
            Category = entity.Product?.Category ?? "",
            Brand = entity.Product?.Brand,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
        };
    }
}

public record InventoryListResponse
{
    public List<InventoryDto> Inventory { get; init; } = [];
    public int Total { get; init; }
    public int Pages { get; init; }
}
