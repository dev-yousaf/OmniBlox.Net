using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Units.DTOs;

namespace OmniBlox.Application.Features.Units.Queries;

public record GetUnitsQuery : IRequest<List<UnitDto>>;

public class GetUnitsQueryHandler : IRequestHandler<GetUnitsQuery, List<UnitDto>>
{
    private readonly IApplicationDbContext _context;
    public GetUnitsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<UnitDto>> Handle(GetUnitsQuery request, CancellationToken ct)
    {
        var items = await _context.Units.OrderBy(x => x.Name).ToListAsync(ct);
        return items.Select(UnitDto.FromEntity).ToList();
    }
}
