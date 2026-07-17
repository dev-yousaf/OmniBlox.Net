using OmniBlox.Domain.Entities;

namespace OmniBlox.Application.Features.Purchases.DTOs;

public record PurchaseOrderItemDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string? ProductName { get; init; }
    public string? ProductSku { get; init; }
    public int Quantity { get; init; }
    public int ReturnedQuantity { get; init; }
    public decimal UnitCost { get; init; }
    public decimal Total { get; init; }
}

public record PurchaseOrderSummaryDto
{
    public Guid Id { get; init; }
    public string ReferenceNumber { get; init; } = string.Empty;
    public string? BillNumber { get; init; }
    public DateTime? BillDate { get; init; }
    public DateTime? DueDate { get; init; }
    public string PaymentStatus { get; init; } = "PENDING";
    public string? PaymentMethod { get; init; }
    public DateTime OrderDate { get; init; }
    public string Status { get; init; } = "PENDING";
    public bool HasReturns { get; init; }
    public Guid SupplierId { get; init; }
    public string SupplierName { get; init; } = string.Empty;
    public Guid? WarehouseId { get; init; }
    public string? WarehouseName { get; init; }
    public decimal Subtotal { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal NetTotal { get; init; }
    public decimal ReturnedValue { get; init; }
    public string? ReturnStatus { get; init; }
    public int PendingReturnCount { get; init; }
    public int ProcessingReturnCount { get; init; }
    public int CompletedReturnCount { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record PurchaseOrderDetailDto : PurchaseOrderSummaryDto
{
    public string? Notes { get; init; }
    public List<PurchaseOrderItemDto> Items { get; init; } = [];
}

public record PurchaseListResponse
{
    public List<PurchaseOrderSummaryDto> Purchases { get; init; } = [];
    public int Total { get; init; }
    public int Pages { get; init; }
}

public record PurchaseOrderStatsDto
{
    public int TotalPurchases { get; init; }
    public decimal TotalCost { get; init; }
    public decimal PendingAmount { get; init; }
    public decimal PaidAmount { get; init; }
    public int PendingCount { get; init; }
    public int ReceivedCount { get; init; }
    public decimal OverdueAmount { get; init; }
}

public record CreatePurchaseItem
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitCost { get; init; }
}

public record CreatePurchaseOrderRequest
{
    public Guid SupplierId { get; init; }
    public DateTime OrderDate { get; init; }
    public string? ReferenceNumber { get; init; }
    public string? BillNumber { get; init; }
    public DateTime? BillDate { get; init; }
    public DateTime? DueDate { get; init; }
    public string? PaymentStatus { get; init; }
    public string? PaymentMethod { get; init; }
    public string? Status { get; init; }
    public string? Notes { get; init; }
    public Guid? WarehouseId { get; init; }
    public List<CreatePurchaseItem> Items { get; init; } = [];
}

public record UpdatePurchaseOrderRequest : CreatePurchaseOrderRequest;
