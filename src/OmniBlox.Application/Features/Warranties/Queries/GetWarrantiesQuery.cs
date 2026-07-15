using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Warranties.DTOs;

namespace OmniBlox.Application.Features.Warranties.Queries;

public record GetWarrantiesQuery : IRequest<List<WarrantyDto>>;

public class GetWarrantiesQueryHandler : IRequestHandler<GetWarrantiesQuery, List<WarrantyDto>>
{
    private readonly IApplicationDbContext _context;
    public GetWarrantiesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<WarrantyDto>> Handle(GetWarrantiesQuery request, CancellationToken ct)
    {
        var items = await _context.Warranties.OrderBy(x => x.Name).ToListAsync(ct);
        return items.Select(WarrantyDto.FromEntity).ToList();
    }
}
