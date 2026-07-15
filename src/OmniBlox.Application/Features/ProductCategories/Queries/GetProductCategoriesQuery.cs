using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.ProductCategories.DTOs;

namespace OmniBlox.Application.Features.ProductCategories.Queries;

public record GetProductCategoriesQuery : IRequest<List<ProductCategoryDto>>;

public class GetProductCategoriesQueryHandler : IRequestHandler<GetProductCategoriesQuery, List<ProductCategoryDto>>
{
    private readonly IApplicationDbContext _context;
    public GetProductCategoriesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<ProductCategoryDto>> Handle(GetProductCategoriesQuery request, CancellationToken ct)
    {
        var items = await _context.ProductCategories
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
        return items.Select(ProductCategoryDto.FromEntity).ToList();
    }
}
