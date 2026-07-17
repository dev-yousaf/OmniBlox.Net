namespace OmniBlox.Application.Features.Quotations.DTOs;

public record QuotationItemDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string? ProductName { get; init; }
    public string? ProductSku { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal Total { get; init; }

    public static QuotationItemDto FromEntity(Domain.Entities.QuotationItem item) => new()
    {
        Id = item.Id,
        ProductId = item.ProductId,
        ProductName = item.Product?.Name,
        ProductSku = item.Product?.SKU,
        Quantity = item.Quantity,
        UnitPrice = item.UnitPrice,
        Total = item.Quantity * item.UnitPrice,
    };
}

public record QuotationSummaryDto
{
    public Guid Id { get; init; }
    public string ReferenceNumber { get; init; } = string.Empty;
    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public DateTime QuoteDate { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public DateTime CreatedAt { get; init; }

    public static QuotationSummaryDto FromEntity(Domain.Entities.Quotation q) => new()
    {
        Id = q.Id,
        ReferenceNumber = q.ReferenceNumber,
        CustomerId = q.CustomerId,
        CustomerName = q.Customer?.Name ?? string.Empty,
        QuoteDate = q.QuoteDate,
        ExpiryDate = q.ExpiryDate,
        Status = q.Status,
        TotalAmount = q.TotalAmount,
        CreatedAt = q.CreatedAt,
    };
}

public record QuotationDetailDto : QuotationSummaryDto
{
    public string? Notes { get; init; }
    public List<QuotationItemDto> Items { get; init; } = [];

    public static new QuotationDetailDto FromEntity(Domain.Entities.Quotation q) => new()
    {
        Id = q.Id,
        ReferenceNumber = q.ReferenceNumber,
        CustomerId = q.CustomerId,
        CustomerName = q.Customer?.Name ?? string.Empty,
        QuoteDate = q.QuoteDate,
        ExpiryDate = q.ExpiryDate,
        Status = q.Status,
        TotalAmount = q.TotalAmount,
        Notes = q.Notes,
        CreatedAt = q.CreatedAt,
        Items = q.Items?.Select(QuotationItemDto.FromEntity).ToList() ?? [],
    };
}

public record QuotationListResponse
{
    public List<QuotationSummaryDto> Quotations { get; init; } = [];
    public int Total { get; init; }
    public int Pages { get; init; }
}

public record CreateQuotationRequest
{
    public Guid CustomerId { get; init; }
    public DateTime QuoteDate { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public string? Status { get; init; }
    public string? Notes { get; init; }
    public List<CreateQuotationItem> Items { get; init; } = [];
}

public record CreateQuotationItem
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}

public record UpdateQuotationRequest : CreateQuotationRequest
{
}
