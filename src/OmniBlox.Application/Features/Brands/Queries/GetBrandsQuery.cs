using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Brands.DTOs;

namespace OmniBlox.Application.Features.Brands.Queries;

public record GetBrandsQuery : IRequest<List<BrandDto>>;

public class GetBrandsQueryHandler : IRequestHandler<GetBrandsQuery, List<BrandDto>>
{
    private readonly IApplicationDbContext _context;
    public GetBrandsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<BrandDto>> Handle(GetBrandsQuery request, CancellationToken ct)
    {
        var items = await _context.Brands.OrderBy(x => x.Name).ToListAsync(ct);
        return items.Select(BrandDto.FromEntity).ToList();
    }
}
