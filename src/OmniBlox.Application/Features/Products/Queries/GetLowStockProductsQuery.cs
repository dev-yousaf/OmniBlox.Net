using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Products.DTOs;
using OmniBlox.Domain.Enums;

namespace OmniBlox.Application.Features.Products.Queries;

public record GetLowStockProductsQuery : IRequest<List<ProductDto>>;

public class GetLowStockProductsQueryHandler : IRequestHandler<GetLowStockProductsQuery, List<ProductDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetLowStockProductsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<ProductDto>> Handle(GetLowStockProductsQuery request, CancellationToken ct)
    {
        var items = await _context.Products
            .Where(p => p.CompanyId == _currentUser.CompanyId)
            .Where(p => p.Status == ProductStatus.ACTIVE)
            .OrderBy(p => p.Stock)
            .ToListAsync(ct);

        // Compute live stock from warehouse_stock
        var productIds = items.Select(p => p.Id).ToList();
        var stockLookup = await _context.Inventories
            .Where(i => productIds.Contains(i.ProductId))
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, TotalStock = g.Sum(i => i.Quantity) })
            .ToDictionaryAsync(x => x.ProductId, x => x.TotalStock, ct);

        var result = new List<ProductDto>();
        foreach (var product in items)
        {
            var liveStock = stockLookup.GetValueOrDefault(product.Id, 0);
            if (liveStock <= product.ReorderLevel)
            {
                product.Stock = liveStock;
                result.Add(ProductDto.FromEntity(product));
            }
        }

        return result.OrderBy(p => p.Stock).ToList();
    }
}
