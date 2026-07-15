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
    public GetLowStockProductsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<ProductDto>> Handle(GetLowStockProductsQuery request, CancellationToken ct)
    {
        var items = await _context.Products
            .Where(p => p.Status == ProductStatus.ACTIVE && p.Stock <= p.ReorderLevel)
            .OrderBy(p => p.Stock)
            .ToListAsync(ct);
        return items.Select(ProductDto.FromEntity).ToList();
    }
}
