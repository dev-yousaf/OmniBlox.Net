using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Products.DTOs;

namespace OmniBlox.Application.Features.Products.Queries;

public record GetUserProductsQuery : IRequest<List<ProductDto>>
{
    public Guid UserId { get; init; }
}

public class GetUserProductsQueryHandler : IRequestHandler<GetUserProductsQuery, List<ProductDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetUserProductsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<ProductDto>> Handle(GetUserProductsQuery request, CancellationToken ct)
    {
        var items = await _context.Products
            .Include(p => p.CreatedByUser)
            .Where(p => p.CompanyId == _currentUser.CompanyId && p.CreatedById == request.UserId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(50)
            .ToListAsync(ct);

        var productIds = items.Select(p => p.Id).ToList();
        var stockLookup = await _context.Inventories
            .Where(i => productIds.Contains(i.ProductId))
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, TotalStock = g.Sum(i => i.Quantity) })
            .ToDictionaryAsync(x => x.ProductId, x => x.TotalStock, ct);

        foreach (var product in items)
        {
            product.Stock = stockLookup.TryGetValue(product.Id, out var liveStock) ? liveStock : 0;
        }

        return items.Select(p => ProductDto.FromEntity(p)).ToList();
    }
}
