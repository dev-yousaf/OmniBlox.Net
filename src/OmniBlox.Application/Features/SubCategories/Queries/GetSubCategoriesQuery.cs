using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.SubCategories.DTOs;

namespace OmniBlox.Application.Features.SubCategories.Queries;

public record GetSubCategoriesQuery : IRequest<List<SubCategoryDto>>
{
    public Guid? CategoryId { get; init; }
}

public class GetSubCategoriesQueryHandler : IRequestHandler<GetSubCategoriesQuery, List<SubCategoryDto>>
{
    private readonly IApplicationDbContext _context;
    public GetSubCategoriesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<SubCategoryDto>> Handle(GetSubCategoriesQuery request, CancellationToken ct)
    {
        var query = _context.SubCategories
            .Include(x => x.Category)
            .AsQueryable();

        if (request.CategoryId.HasValue)
            query = query.Where(x => x.CategoryId == request.CategoryId.Value);

        var items = await query.OrderBy(x => x.Name).ToListAsync(ct);
        return items.Select(SubCategoryDto.FromEntity).ToList();
    }
}
