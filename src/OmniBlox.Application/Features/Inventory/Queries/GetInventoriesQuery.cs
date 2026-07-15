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
    public GetInventoriesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<InventoryListResponse> Handle(GetInventoriesQuery request, CancellationToken ct)
    {
        var hasInventoryRecords = await _context.Inventories.AnyAsync(ct);
        if (!hasInventoryRecords)
            return await HandleFallback(request, ct);

        var query = _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .AsQueryable();

        if (request.WarehouseId.HasValue)
            query = query.Where(i => i.WarehouseId == request.WarehouseId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.ToLower();
            query = query.Where(i => i.Product.Name.ToLower().Contains(s) || i.Product.SKU.ToLower().Contains(s));
        }

        if (request.Filter == "low_stock")
            query = query.Where(i => i.Quantity > 0 && i.Quantity <= i.Product.ReorderLevel);
        else if (request.Filter == "out_of_stock")
            query = query.Where(i => i.Quantity <= 0);

        var total = await query.CountAsync(ct);
        var pages = (int)Math.Ceiling(total / (double)request.Limit);

        var items = await query
            .OrderBy(i => i.Product.Name)
            .Skip((request.Page - 1) * request.Limit)
            .Take(request.Limit)
            .ToListAsync(ct);

        return new InventoryListResponse
        {
            Inventory = items.Select(InventoryDto.FromEntity).ToList(),
            Total = total,
            Pages = pages,
        };
    }

    private async Task<InventoryListResponse> HandleFallback(GetInventoriesQuery request, CancellationToken ct)
    {
        var productsQuery = _context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.ToLower();
            productsQuery = productsQuery.Where(p => p.Name.ToLower().Contains(s) || p.SKU.ToLower().Contains(s));
        }

        if (request.Filter == "out_of_stock")
            productsQuery = productsQuery.Where(p => p.Stock <= 0);
        else if (request.Filter == "low_stock")
            productsQuery = productsQuery.Where(p => p.Stock > 0 && p.Stock <= p.ReorderLevel);

        var warehouses = await _context.Warehouses.OrderBy(w => w.Name).ToListAsync(ct);
        var warehouse = warehouses.FirstOrDefault();

        var total = await productsQuery.CountAsync(ct);
        var pages = (int)Math.Ceiling(total / (double)request.Limit);

        var products = await productsQuery
            .OrderBy(p => p.Name)
            .Skip((request.Page - 1) * request.Limit)
            .Take(request.Limit)
            .ToListAsync(ct);

        var dtos = products.Select(p => new InventoryDto
        {
            ProductId = p.Id,
            ProductName = p.Name,
            ProductSku = p.SKU,
            ImageUrl = p.ImageUrl,
            WarehouseId = warehouse?.Id ?? Guid.Empty,
            WarehouseName = warehouse?.Name ?? "Default",
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

        return new InventoryListResponse
        {
            Inventory = dtos,
            Total = total,
            Pages = pages,
        };
    }
}
