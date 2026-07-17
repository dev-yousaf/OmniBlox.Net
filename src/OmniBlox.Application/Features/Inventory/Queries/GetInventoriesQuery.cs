using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Inventory.DTOs;

namespace OmniBlox.Application.Features.Inventory.Queries;

public record GetInventoriesQuery : IRequest<InventoryListResponse>
{
    public string? Search { get; init; }
    public Guid? WarehouseId { get; init; }
    public string? Filter { get; init; }
    public int Page { get; init; } = 1;
    public int Limit { get; init; } = 20;
}

public class GetInventoriesQueryHandler : IRequestHandler<GetInventoriesQuery, InventoryListResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetInventoriesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<InventoryListResponse> Handle(GetInventoriesQuery request, CancellationToken ct)
    {
        var warehouses = await _context.Warehouses
            .Where(w => w.CompanyId == _currentUser.CompanyId)
            .OrderBy(w => w.Name)
            .ToListAsync(ct);
        var defaultWarehouse = warehouses.FirstOrDefault();

        // Inventory records (per-warehouse)
        var inventoryQuery = _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .AsQueryable();

        if (request.WarehouseId.HasValue)
            inventoryQuery = inventoryQuery.Where(i => i.WarehouseId == request.WarehouseId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.ToLower();
            inventoryQuery = inventoryQuery.Where(i => i.Product.Name.ToLower().Contains(s) || i.Product.SKU.ToLower().Contains(s));
        }

        // Inventory filter
        if (request.Filter == "out_of_stock")
            inventoryQuery = inventoryQuery.Where(i => i.Quantity <= 0);
        else if (request.Filter == "low_stock")
            inventoryQuery = inventoryQuery.Where(i => i.Quantity > 0 && i.Quantity <= i.Product.ReorderLevel);

        var inventoryItems = await inventoryQuery
            .OrderBy(i => i.Product.Name)
            .ToListAsync(ct);

        var inventoryDtos = inventoryItems.Select(InventoryDto.FromEntity).ToList();

        // Products without any inventory record (only when no warehouse filter)
        if (!request.WarehouseId.HasValue && defaultWarehouse is not null)
        {
            var productIdsWithInventory = await _context.Inventories
                .Select(i => i.ProductId)
                .Distinct()
                .ToListAsync(ct);

            var productsWithoutInventoryQuery = _context.Products
                .Where(p => !productIdsWithInventory.Contains(p.Id))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.ToLower();
                productsWithoutInventoryQuery = productsWithoutInventoryQuery.Where(p =>
                    p.Name.ToLower().Contains(s) || p.SKU.ToLower().Contains(s));
            }

            var productsWithoutInventory = await productsWithoutInventoryQuery
                .OrderBy(p => p.Name)
                .ToListAsync(ct);

            // Products without inventory have zero stock in the warehouse
            var productDtos = productsWithoutInventory.Select(p => new InventoryDto
            {
                ProductId = p.Id,
                ProductName = p.Name,
                ProductSku = p.SKU,
                ImageUrl = p.ImageUrl,
                WarehouseId = defaultWarehouse.Id,
                WarehouseName = defaultWarehouse.Name ?? "Default",
                Quantity = 0,
                SalePrice = p.SalePrice,
                CostPrice = p.CostPrice,
                ReorderLevel = p.ReorderLevel,
                StockValue = 0,
                Status = "out_of_stock",
                Category = p.Category,
                Brand = p.Brand,
                UpdatedAt = p.UpdatedAt,
            }).ToList();

            inventoryDtos.AddRange(productDtos);
        }

        // Sort, paginate
        var all = inventoryDtos
            .OrderBy(d => d.ProductName)
            .ToList();

        var total = all.Count;
        var pages = (int)Math.Ceiling(total / (double)request.Limit);

        var paginated = all
            .Skip((request.Page - 1) * request.Limit)
            .Take(request.Limit)
            .ToList();

        return new InventoryListResponse
        {
            Inventory = paginated,
            Total = total,
            Pages = pages,
        };
    }
}
