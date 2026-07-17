using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Inventory.DTOs;

namespace OmniBlox.Application.Features.Inventory.Queries;

public record GetWarehouseInventoryQuery : IRequest<WarehouseInventoryDto>
{
    public Guid WarehouseId { get; init; }
}

public class GetWarehouseInventoryQueryHandler : IRequestHandler<GetWarehouseInventoryQuery, WarehouseInventoryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetWarehouseInventoryQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<WarehouseInventoryDto> Handle(GetWarehouseInventoryQuery request, CancellationToken ct)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == request.WarehouseId && w.CompanyId == _currentUser.CompanyId, ct);
        if (warehouse is null)
            throw new KeyNotFoundException("Warehouse not found.");

        var inventories = await _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Where(i => i.WarehouseId == request.WarehouseId)
            .OrderBy(i => i.Product.Name)
            .ToListAsync(ct);

        var dtos = inventories.Select(InventoryDto.FromEntity).ToList();

        return new WarehouseInventoryDto
        {
            WarehouseId = warehouse.Id,
            WarehouseName = warehouse.Name,
            Location = warehouse.Location,
            TotalProducts = dtos.Count,
            TotalStockValue = dtos.Sum(d => d.StockValue),
            LowStockCount = dtos.Count(d => d.Status == "low_stock"),
            OutOfStockCount = dtos.Count(d => d.Status == "out_of_stock"),
            Inventory = dtos,
        };
    }
}
