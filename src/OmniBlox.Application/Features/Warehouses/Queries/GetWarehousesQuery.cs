using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Warehouses.DTOs;

namespace OmniBlox.Application.Features.Warehouses.Queries;

public record GetWarehousesQuery : IRequest<List<WarehouseDto>>;

public class GetWarehousesQueryHandler : IRequestHandler<GetWarehousesQuery, List<WarehouseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetWarehousesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<WarehouseDto>> Handle(GetWarehousesQuery request, CancellationToken ct)
    {
        var items = await _context.Warehouses
            .Where(w => w.CompanyId == _currentUser.CompanyId)
            .Include(w => w.Inventories)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);
        return items.Select(w =>
        {
            var dto = WarehouseDto.FromEntity(w);
            dto = dto with { Count = new WarehouseCount { ProductCount = w.Inventories.Count } };
            return dto;
        }).ToList();
    }
}
