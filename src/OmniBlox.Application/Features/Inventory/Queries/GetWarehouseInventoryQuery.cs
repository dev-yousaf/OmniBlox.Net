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
    public GetWarehouseInventoryQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<WarehouseInventoryDto> Handle(GetWarehouseInventoryQuery request, CancellationToken ct)
    {
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == request.WarehouseId, ct);
        if (warehouse is null)
            throw new KeyNotFoundException("Warehouse not found.");

        var inventories = await _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Where(i => i.WarehouseId == request.WarehouseId)
            .OrderBy(i => i.Product.Name)
            .ToListAsync(ct);

        List<InventoryDto> dtos;

        if (inventories.Count > 0)
        {
            dtos = inventories.Select(InventoryDto.FromEntity).ToList();
        }
        else
        {
            var products = await _context.Products
                .OrderBy(p => p.Name)
                .Where(p => p.Stock > 0)
                .ToListAsync(ct);

            dtos = products.Select(p => new InventoryDto
            {
                ProductId = p.Id,
                ProductName = p.Name,
                ProductSku = p.SKU,
                ImageUrl = p.ImageUrl,
                WarehouseId = warehouse.Id,
                WarehouseName = warehouse.Name,
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
