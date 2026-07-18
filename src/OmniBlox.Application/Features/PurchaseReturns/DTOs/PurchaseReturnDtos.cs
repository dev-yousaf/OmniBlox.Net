namespace OmniBlox.Application.Features.PurchaseReturns.DTOs;

public record PurchaseReturnItemDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string? ProductName { get; init; }
    public string? ProductSku { get; init; }
    public int Quantity { get; init; }
    public decimal UnitCost { get; init; }
    public decimal Total { get; init; }
    public Guid? PurchaseOrderItemId { get; init; }
}

public record PurchaseReturnSummaryDto
{
    public Guid Id { get; init; }
    public string ReferenceNumber { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public string? Reason { get; init; }
    public string Status { get; init; } = "PENDING";
    public DateTime ReturnDate { get; init; }
    public Guid WarehouseId { get; init; }
    public string WarehouseName { get; init; } = string.Empty;
    public Guid SupplierId { get; init; }
    public string SupplierName { get; init; } = string.Empty;
    public Guid? PurchaseOrderId { get; init; }
    public string? PurchaseOrderReference { get; init; }
    public int ItemCount { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record PurchaseReturnDetailDto : PurchaseReturnSummaryDto
{
    public List<PurchaseReturnItemDto> Items { get; init; } = [];
}

public record PurchaseReturnListResponse
{
    public List<PurchaseReturnSummaryDto> Returns { get; init; } = [];
    public int Total { get; init; }
    public int Pages { get; init; }
}

public record CreatePurchaseReturnItem
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitCost { get; init; }
    public Guid? PurchaseOrderItemId { get; init; }
}

public record CreatePurchaseReturnRequest
{
    public Guid WarehouseId { get; init; }
    public Guid SupplierId { get; init; }
    public Guid? PurchaseOrderId { get; init; }
    public string? Reason { get; init; }
    public DateTime ReturnDate { get; init; }
    public List<CreatePurchaseReturnItem> Items { get; init; } = [];
}

public record UpdatePurchaseReturnStatusRequest
{
    public string Status { get; init; } = "COMPLETED";
}

public record UpdatePurchaseReturnRequest
{
    public Guid WarehouseId { get; init; }
    public Guid SupplierId { get; init; }
    public Guid? PurchaseOrderId { get; init; }
    public string? Reason { get; init; }
    public DateTime ReturnDate { get; init; }
    public List<CreatePurchaseReturnItem> Items { get; init; } = [];
}
