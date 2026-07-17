using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Brands.DTOs;

namespace OmniBlox.Application.Features.Brands.Queries;

public record GetBrandsQuery : IRequest<List<BrandDto>>;

public class GetBrandsQueryHandler : IRequestHandler<GetBrandsQuery, List<BrandDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetBrandsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<BrandDto>> Handle(GetBrandsQuery request, CancellationToken ct)
    {
        var items = await _context.Brands.Where(e => e.CompanyId == _currentUser.CompanyId).OrderBy(x => x.Name).ToListAsync(ct);
        return items.Select(BrandDto.FromEntity).ToList();
    }
}
