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
    private readonly ICurrentUserService _currentUser;
    public GetSubCategoriesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<SubCategoryDto>> Handle(GetSubCategoriesQuery request, CancellationToken ct)
    {
        var query = _context.SubCategories
            .Include(x => x.Category)
            .AsQueryable()
            .Where(x => x.CompanyId == _currentUser.CompanyId);

        if (request.CategoryId.HasValue)
            query = query.Where(x => x.CategoryId == request.CategoryId.Value);

        var items = await query.OrderBy(x => x.Name).ToListAsync(ct);
        return items.Select(SubCategoryDto.FromEntity).ToList();
    }
}
