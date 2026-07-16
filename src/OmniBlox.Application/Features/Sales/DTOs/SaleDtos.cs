using OmniBlox.Domain.Entities;

namespace OmniBlox.Application.Features.Sales.DTOs;

public record SaleItemDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string? ProductName { get; init; }
    public int Quantity { get; init; }
    public int ReturnedQuantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal Total { get; init; }
}

public record SaleSummaryDto
{
    public Guid Id { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string? CustomerEmail { get; init; }
    public DateTime SaleDate { get; init; }
    public DateTime DueDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public string PaymentStatus { get; init; } = string.Empty;
    public string? PaymentMethod { get; init; }
    public Guid? WarehouseId { get; init; }
    public string? WarehouseName { get; init; }
    public bool HasReturns { get; init; }
    public decimal Subtotal { get; init; }
    public decimal Tax { get; init; }
    public decimal Discount { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal BalanceDue => PaymentStatus == "PAID" ? 0 : TotalAmount;
    public decimal NetTotal => TotalAmount - ReturnedValue;
    public decimal ReturnedValue { get; init; }
    public string ReturnStatus => HasReturns ? "PARTIAL" : "NONE";
    public bool IsOverdue => DueDate < DateTime.UtcNow && PaymentStatus != "PAID";
    public int PendingReturnCount { get; init; }
    public int ProcessingReturnCount { get; init; }
    public int CompletedReturnCount { get; init; }
    public DateTime CreatedAt { get; init; }

    public static SaleSummaryDto FromEntity(Sale sale) => new()
    {
        Id = sale.Id,
        InvoiceNumber = sale.InvoiceNumber,
        CustomerId = sale.CustomerId,
        CustomerName = sale.Customer?.Name ?? string.Empty,
        CustomerEmail = sale.Customer?.Email,
        SaleDate = sale.SaleDate,
        DueDate = sale.DueDate,
        Status = sale.Status,
        PaymentStatus = sale.PaymentStatus,
        PaymentMethod = sale.PaymentMethod,
        WarehouseId = sale.WarehouseId,
        WarehouseName = sale.Warehouse?.Name,
        HasReturns = sale.HasReturns,
        Subtotal = sale.Subtotal,
        Tax = sale.Tax,
        Discount = sale.Discount,
        TotalAmount = sale.TotalAmount,
        CreatedAt = sale.CreatedAt,
    };
}

public record SaleDetailDto : SaleSummaryDto
{
    public string? Notes { get; init; }
    public List<SaleItemDto> Items { get; init; } = new();

    public static new SaleDetailDto FromEntity(Sale sale) => new()
    {
        Id = sale.Id,
        InvoiceNumber = sale.InvoiceNumber,
        CustomerId = sale.CustomerId,
        CustomerName = sale.Customer?.Name ?? string.Empty,
        CustomerEmail = sale.Customer?.Email,
        SaleDate = sale.SaleDate,
        DueDate = sale.DueDate,
        Status = sale.Status,
        PaymentStatus = sale.PaymentStatus,
        PaymentMethod = sale.PaymentMethod,
        WarehouseId = sale.WarehouseId,
        WarehouseName = sale.Warehouse?.Name,
        HasReturns = sale.HasReturns,
        Subtotal = sale.Subtotal,
        Tax = sale.Tax,
        Discount = sale.Discount,
        TotalAmount = sale.TotalAmount,
        Notes = sale.Notes,
        CreatedAt = sale.CreatedAt,
        Items = sale.Items.Select(i => new SaleItemDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.Product?.Name,
            Quantity = i.Quantity,
            ReturnedQuantity = i.ReturnedQuantity,
            UnitPrice = i.UnitPrice,
            Total = i.Quantity * i.UnitPrice,
        }).ToList(),
    };
}

public record SalesListResponse
{
    public List<SaleSummaryDto> Sales { get; init; } = new();
    public int Total { get; init; }
    public int Pages { get; init; }
}

public record SalesStatsDto
{
    public int TotalSales { get; init; }
    public decimal TotalRevenue { get; init; }
    public decimal PendingAmount { get; init; }
    public decimal OverdueAmount { get; init; }
    public int PaidInvoices { get; init; }
    public int PendingInvoices { get; init; }
    public int OverdueInvoices { get; init; }
}

public record CreateSaleItem
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}

public record CreateSaleRequest
{
    public string? InvoiceNumber { get; init; }
    public Guid CustomerId { get; init; }
    public Guid WarehouseId { get; init; }
    public DateTime SaleDate { get; init; }
    public DateTime DueDate { get; init; }
    public string? Status { get; init; }
    public string? PaymentStatus { get; init; }
    public string? PaymentMethod { get; init; }
    public decimal TaxRate { get; init; }
    public decimal Discount { get; init; }
    public string? Notes { get; init; }
    public string? ShippingAddress { get; init; }
    public List<CreateSaleItem> Items { get; init; } = new();
}

public record UpdateSaleRequest : CreateSaleRequest;
