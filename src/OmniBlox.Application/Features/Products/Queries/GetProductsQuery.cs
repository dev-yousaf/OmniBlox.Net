using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Products.DTOs;

namespace OmniBlox.Application.Features.Products.Queries;

public record GetProductsQuery : IRequest<ProductListResponse>
{
    public int Page { get; init; } = 1;
    public int Limit { get; init; } = 20;
    public string? Search { get; init; }
    public string? Category { get; init; }
    public string? Status { get; init; }
}

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, ProductListResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetProductsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ProductListResponse> Handle(GetProductsQuery request, CancellationToken ct)
    {
        var query = _context.Products.Where(e => e.CompanyId == _currentUser.CompanyId).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(search) ||
                p.SKU.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
            query = query.Where(p => p.Category == request.Category);

        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<Domain.Enums.ProductStatus>(request.Status, true, out var status))
            query = query.Where(p => p.Status == status);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.Limit)
            .Take(request.Limit)
            .ToListAsync(ct);

        // Compute live stock from warehouse_stock to eliminate drift
        var productIds = items.Select(p => p.Id).ToList();
        var stockLookup = await _context.Inventories
            .Where(i => productIds.Contains(i.ProductId))
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, TotalStock = g.Sum(i => i.Quantity) })
            .ToDictionaryAsync(x => x.ProductId, x => x.TotalStock, ct);

        foreach (var product in items)
        {
            if (stockLookup.TryGetValue(product.Id, out var liveStock))
                product.Stock = liveStock;
            else
                product.Stock = 0;
        }

        return new ProductListResponse
        {
            Products = items.Select(p => ProductDto.FromEntity(p)).ToList(),
            Total = total,
            Pages = (int)Math.Ceiling((double)total / request.Limit),
            Page = request.Page,
            Limit = request.Limit,
        };
    }
}
