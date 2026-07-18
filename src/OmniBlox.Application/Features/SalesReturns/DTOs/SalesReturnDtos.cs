using OmniBlox.Domain.Entities;

namespace OmniBlox.Application.Features.SalesReturns.DTOs;

public record SalesReturnItemDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string? ProductName { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal Total { get; init; }
    public Guid? SaleItemId { get; init; }
}

public record SalesReturnSummaryDto
{
    public Guid Id { get; init; }
    public string ReferenceNumber { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public string? Reason { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime ReturnDate { get; init; }
    public Guid WarehouseId { get; init; }
    public string? WarehouseName { get; init; }
    public Guid? SaleId { get; init; }
    public string? SaleInvoiceNumber { get; init; }
    public int ItemCount { get; init; }
    public DateTime CreatedAt { get; init; }

    public static SalesReturnSummaryDto FromEntity(SalesReturn entity) => new()
    {
        Id = entity.Id,
        ReferenceNumber = entity.ReferenceNumber,
        TotalAmount = entity.TotalAmount,
        Reason = entity.Reason,
        Status = entity.Status,
        ReturnDate = entity.ReturnDate,
        WarehouseId = entity.WarehouseId,
        WarehouseName = entity.Warehouse?.Name,
        SaleId = entity.SaleId,
        SaleInvoiceNumber = entity.Sale?.InvoiceNumber,
        ItemCount = entity.Items.Count,
        CreatedAt = entity.CreatedAt,
    };
}

public record SalesReturnDetailDto : SalesReturnSummaryDto
{
    public List<SalesReturnItemDto> Items { get; init; } = new();

    public static new SalesReturnDetailDto FromEntity(SalesReturn entity) => new()
    {
        Id = entity.Id,
        ReferenceNumber = entity.ReferenceNumber,
        TotalAmount = entity.TotalAmount,
        Reason = entity.Reason,
        Status = entity.Status,
        ReturnDate = entity.ReturnDate,
        WarehouseId = entity.WarehouseId,
        WarehouseName = entity.Warehouse?.Name,
        SaleId = entity.SaleId,
        SaleInvoiceNumber = entity.Sale?.InvoiceNumber,
        CreatedAt = entity.CreatedAt,
        Items = entity.Items.Select(i => new SalesReturnItemDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.Product?.Name,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            Total = i.Quantity * i.UnitPrice,
            SaleItemId = i.SaleItemId,
        }).ToList(),
    };
}

public record SalesReturnsListResponse
{
    public List<SalesReturnSummaryDto> Returns { get; init; } = new();
    public int Total { get; init; }
    public int Pages { get; init; }
}

public record CreateSalesReturnItem
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public Guid? SaleItemId { get; init; }
}

public record CreateSalesReturnRequest
{
    public Guid WarehouseId { get; init; }
    public Guid? SaleId { get; init; }
    public string? Reason { get; init; }
    public DateTime ReturnDate { get; init; }
    public List<CreateSalesReturnItem> Items { get; init; } = new();
}

public record UpdateSalesReturnStatusRequest
{
    public string Status { get; init; } = string.Empty;
}

public record UpdateSalesReturnRequest
{
    public Guid WarehouseId { get; init; }
    public Guid? SaleId { get; init; }
    public string? Reason { get; init; }
    public DateTime ReturnDate { get; init; }
    public List<CreateSalesReturnItem> Items { get; init; } = new();
}
