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
    public GetProductStatsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<ProductStatsResponse> Handle(GetProductStatsQuery request, CancellationToken ct)
    {
        var products = await _context.Products.Where(p => p.Status == ProductStatus.ACTIVE).ToListAsync(ct);

        return new ProductStatsResponse
        {
            TotalProducts = products.Count,
            LowStockCount = products.Count(p => p.Stock <= p.ReorderLevel),
            TotalValue = products.Sum(p => p.Stock * p.CostPrice),
            CategoriesCount = products.Select(p => p.Category).Distinct().Count(),
        };
    }
}
