using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Warehouses.DTOs;

namespace OmniBlox.Application.Features.Warehouses.Queries;

public record GetWarehousesQuery : IRequest<List<WarehouseDto>>;

public class GetWarehousesQueryHandler : IRequestHandler<GetWarehousesQuery, List<WarehouseDto>>
{
    private readonly IApplicationDbContext _context;
    public GetWarehousesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<WarehouseDto>> Handle(GetWarehousesQuery request, CancellationToken ct)
    {
        var items = await _context.Warehouses
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
