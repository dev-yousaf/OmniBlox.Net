using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Products.DTOs;
using OmniBlox.Domain.Enums;

namespace OmniBlox.Application.Features.Products.Queries;

public record GetLowStockDetailsQuery : IRequest<LowStockDetailsResponse>
{
    public int Page { get; init; } = 1;
    public int Limit { get; init; } = 20;
}

public class GetLowStockDetailsQueryHandler : IRequestHandler<GetLowStockDetailsQuery, LowStockDetailsResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetLowStockDetailsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<LowStockDetailsResponse> Handle(GetLowStockDetailsQuery request, CancellationToken ct)
    {
        var companyProducts = await _context.Products
            .Where(e => e.CompanyId == _currentUser.CompanyId)
            .Where(p => p.Status == ProductStatus.ACTIVE)
            .ToListAsync(ct);

        var productIds = companyProducts.Select(p => p.Id).ToList();
        var stockLookup = await _context.Inventories
            .Where(i => productIds.Contains(i.ProductId))
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, TotalStock = g.Sum(i => i.Quantity) })
            .ToDictionaryAsync(x => x.ProductId, x => x.TotalStock, ct);

        var lowStockItems = companyProducts
            .Select(p => new
            {
                Product = p,
                LiveStock = stockLookup.GetValueOrDefault(p.Id, 0),
            })
            .Where(x => x.LiveStock <= x.Product.ReorderLevel)
            .OrderBy(x => x.LiveStock)
            .ToList();

        var total = lowStockItems.Count;
        var paged = lowStockItems
            .Skip((request.Page - 1) * request.Limit)
            .Take(request.Limit)
            .Select(x => new LowStockDetailItem
            {
                ProductId = x.Product.Id,
                ProductName = x.Product.Name,
                Sku = x.Product.SKU,
                ImageUrl = x.Product.ImageUrl,
                Category = x.Product.Category,
                Quantity = x.LiveStock,
                AlertQuantity = x.Product.ReorderLevel,
            })
            .ToList();

        return new LowStockDetailsResponse
        {
            Items = paged,
            Total = total,
            Pages = (int)Math.Ceiling((double)total / request.Limit),
        };
    }
}
