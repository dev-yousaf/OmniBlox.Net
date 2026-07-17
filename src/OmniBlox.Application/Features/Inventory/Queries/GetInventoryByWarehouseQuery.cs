using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Inventory.DTOs;

namespace OmniBlox.Application.Features.Inventory.Queries;

public record GetInventoryByWarehouseQuery : IRequest<List<InventoryDto>>
{
    public Guid WarehouseId { get; init; }
}

public class GetInventoryByWarehouseQueryHandler : IRequestHandler<GetInventoryByWarehouseQuery, List<InventoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetInventoryByWarehouseQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<InventoryDto>> Handle(GetInventoryByWarehouseQuery request, CancellationToken ct)
    {
        var items = await _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Where(i => i.WarehouseId == request.WarehouseId)
            .OrderBy(i => i.Product.Name)
            .ToListAsync(ct);

        if (items.Count > 0)
            return items.Select(InventoryDto.FromEntity).ToList();

        var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == request.WarehouseId, ct);
        var whName = warehouse?.Name ?? "Unknown";

        var products = await _context.Products
            .OrderBy(p => p.Name)
            .ToListAsync(ct);

        return products.Select(p => new InventoryDto
        {
            ProductId = p.Id,
            ProductName = p.Name,
            ProductSku = p.SKU,
            ImageUrl = p.ImageUrl,
            WarehouseId = request.WarehouseId,
            WarehouseName = whName,
            Quantity = p.Stock,
            SalePrice = p.SalePrice,
            CostPrice = p.CostPrice,
            ReorderLevel = p.ReorderLevel,
            StockValue = p.Stock * p.CostPrice,
            Status = p.Stock <= 0 ? "out_of_stock" : p.Stock <= p.ReorderLevel ? "low_stock" : "in_stock",
            Category = p.Category,
            Brand = p.Brand,
            UpdatedAt = p.UpdatedAt,
        }).ToList();
    }
}
