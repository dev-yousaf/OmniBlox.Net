using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Inventory.DTOs;

namespace OmniBlox.Application.Features.Inventory.Queries;

public record GetInventoryStatsQuery : IRequest<InventoryStatsDto>;

public class GetInventoryStatsQueryHandler : IRequestHandler<GetInventoryStatsQuery, InventoryStatsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetInventoryStatsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<InventoryStatsDto> Handle(GetInventoryStatsQuery request, CancellationToken ct)
    {
        var inventories = await _context.Inventories
            .Include(i => i.Product)
            .ToListAsync(ct);

        return new InventoryStatsDto
        {
            TotalProducts = inventories.Select(i => i.ProductId).Distinct().Count(),
            TotalWarehouses = await _context.Warehouses.CountAsync(ct),
            LowStockProducts = inventories.Count(i => i.Quantity > 0 && i.Quantity <= i.Product.ReorderLevel),
            OutOfStockProducts = inventories.Count(i => i.Quantity <= 0),
            TotalStockValue = inventories.Sum(i => i.Quantity * (i.Product?.CostPrice ?? 0)),
            RecentAdjustments = await _context.StockAdjustments.CountAsync(ct),
        };
    }
}
