using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Products.DTOs;
using OmniBlox.Domain.Enums;

namespace OmniBlox.Application.Features.Products.Queries;

public record GetProductStatsQuery : IRequest<ProductStatsResponse>;

public class GetProductStatsQueryHandler : IRequestHandler<GetProductStatsQuery, ProductStatsResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetProductStatsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ProductStatsResponse> Handle(GetProductStatsQuery request, CancellationToken ct)
    {
        var products = await _context.Products
            .Where(p => p.CompanyId == _currentUser.CompanyId)
            .Where(p => p.Status == ProductStatus.ACTIVE)
            .ToListAsync(ct);

        var productIds = products.Select(p => p.Id).ToList();
        var stockLookup = await _context.Inventories
            .Where(i => productIds.Contains(i.ProductId))
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, TotalStock = g.Sum(i => i.Quantity) })
            .ToDictionaryAsync(x => x.ProductId, x => x.TotalStock, ct);

        return new ProductStatsResponse
        {
            TotalProducts = products.Count,
            LowStockCount = products.Count(p =>
            {
                var liveStock = stockLookup.GetValueOrDefault(p.Id, 0);
                return liveStock <= p.ReorderLevel;
            }),
            TotalValue = products.Sum(p =>
            {
                var liveStock = stockLookup.GetValueOrDefault(p.Id, 0);
                return liveStock * p.CostPrice;
            }),
            CategoriesCount = products.Select(p => p.Category).Distinct().Count(),
        };
    }
}
