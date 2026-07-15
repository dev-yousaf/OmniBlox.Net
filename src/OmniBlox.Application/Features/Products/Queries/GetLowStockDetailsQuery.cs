using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Enums;

namespace OmniBlox.Application.Features.Products.Queries;

public record GetLowStockDetailsQuery : IRequest<LowStockDetailsResponse>
{
    public int Page { get; init; } = 1;
    public int Limit { get; init; } = 20;
}

public record LowStockDetailItem
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public string Category { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public int AlertQuantity { get; init; }
}

public record LowStockDetailsResponse
{
    public List<LowStockDetailItem> Items { get; init; } = [];
    public int Total { get; init; }
    public int Pages { get; init; }
}

public class GetLowStockDetailsQueryHandler : IRequestHandler<GetLowStockDetailsQuery, LowStockDetailsResponse>
{
    private readonly IApplicationDbContext _context;
    public GetLowStockDetailsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<LowStockDetailsResponse> Handle(GetLowStockDetailsQuery request, CancellationToken ct)
    {
        var query = _context.Products
            .Where(p => p.Status == ProductStatus.ACTIVE && p.Stock <= p.ReorderLevel);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.Stock)
            .Skip((request.Page - 1) * request.Limit)
            .Take(request.Limit)
            .Select(p => new LowStockDetailItem
            {
                ProductId = p.Id,
                ProductName = p.Name,
                Sku = p.SKU,
                ImageUrl = p.ImageUrl,
                Category = p.Category,
                Quantity = p.Stock,
                AlertQuantity = p.ReorderLevel,
            })
            .ToListAsync(ct);

        return new LowStockDetailsResponse
        {
            Items = items,
            Total = total,
            Pages = (int)Math.Ceiling((double)total / request.Limit),
        };
    }
}
