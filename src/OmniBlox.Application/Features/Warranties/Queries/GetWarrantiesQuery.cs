using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Warranties.DTOs;

namespace OmniBlox.Application.Features.Warranties.Queries;

public record GetWarrantiesQuery : IRequest<List<WarrantyDto>>;

public class GetWarrantiesQueryHandler : IRequestHandler<GetWarrantiesQuery, List<WarrantyDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetWarrantiesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<WarrantyDto>> Handle(GetWarrantiesQuery request, CancellationToken ct)
    {
        var items = await _context.Warranties.Where(e => e.CompanyId == _currentUser.CompanyId).OrderBy(x => x.Name).ToListAsync(ct);
        return items.Select(WarrantyDto.FromEntity).ToList();
    }
}
