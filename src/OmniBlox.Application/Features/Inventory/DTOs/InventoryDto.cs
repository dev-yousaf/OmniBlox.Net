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

public record InventoryStatsDto
{
    public int TotalProducts { get; init; }
    public int TotalWarehouses { get; init; }
    public int LowStockProducts { get; init; }
    public int OutOfStockProducts { get; init; }
    public decimal TotalStockValue { get; init; }
    public int RecentAdjustments { get; init; }
}

public record WarehouseInventoryDto
{
    public Guid WarehouseId { get; init; }
    public string WarehouseName { get; init; } = string.Empty;
    public string? Location { get; init; }
    public int TotalProducts { get; init; }
    public decimal TotalStockValue { get; init; }
    public int LowStockCount { get; init; }
    public int OutOfStockCount { get; init; }
    public List<InventoryDto> Inventory { get; init; } = [];
}

public record TransferStockRequest
{
    public Guid ProductId { get; init; }
    public Guid FromWarehouseId { get; init; }
    public Guid ToWarehouseId { get; init; }
    public int Quantity { get; init; }
    public string? Note { get; init; }
}

public record BulkTransferItem
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}

public record BulkTransferRequest
{
    public Guid FromWarehouseId { get; init; }
    public Guid ToWarehouseId { get; init; }
    public List<BulkTransferItem> Items { get; init; } = [];
    public string? Note { get; init; }
}

public record StockTransferDto
{
    public Guid Id { get; init; }
    public string ReferenceNumber { get; init; } = string.Empty;
    public int TotalItems { get; init; }
    public DateTime AdjustmentDate { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public TransferUserInfo? User { get; init; }
    public List<StockAdjustmentItemDto> Items { get; init; } = [];
}

public record TransferListResponse
{
    public List<StockTransferDto> Transfers { get; init; } = [];
    public int Total { get; init; }
    public int Pages { get; init; }
}

public record CreateStockAdjustmentRequest
{
    public string Type { get; init; } = "ADDITION";
    public List<AdjustmentItem> Items { get; init; } = [];
    public string? Notes { get; init; }
    public string? DocumentUrl { get; init; }
}

public record AdjustmentItem
{
    public Guid ProductId { get; init; }
    public Guid WarehouseId { get; init; }
    public int NewQuantity { get; init; }
}

public record StockAdjustmentDto
{
    public Guid Id { get; init; }
    public string ReferenceNumber { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public int TotalItems { get; init; }
    public int NetChange { get; init; }
    public DateTime AdjustmentDate { get; init; }
    public DateTime CreatedAt { get; init; }
    public TransferUserInfo? User { get; init; }
    public List<StockAdjustmentItemDto> Items { get; init; } = [];
}

public record StockAdjustmentItemDto
{
    public Guid Id { get; init; }
    public int PreviousQuantity { get; init; }
    public int NewQuantity { get; init; }
    public int Difference { get; init; }
    public ItemProductInfo? Product { get; init; }
    public ItemWarehouseInfo? Warehouse { get; init; }
}

public record ItemProductInfo
{
    public string Name { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
}

public record ItemWarehouseInfo
{
    public string Name { get; init; } = string.Empty;
}

public record TransferUserInfo
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}

public record AdjustmentListResponse
{
    public List<StockAdjustmentDto> Adjustments { get; init; } = [];
    public int Total { get; init; }
    public int Pages { get; init; }
}
