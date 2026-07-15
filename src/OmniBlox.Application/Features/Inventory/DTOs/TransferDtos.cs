namespace OmniBlox.Application.Features.Inventory.DTOs;

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
