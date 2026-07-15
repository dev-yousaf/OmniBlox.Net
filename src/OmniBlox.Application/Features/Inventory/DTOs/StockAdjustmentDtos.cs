namespace OmniBlox.Application.Features.Inventory.DTOs;

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
