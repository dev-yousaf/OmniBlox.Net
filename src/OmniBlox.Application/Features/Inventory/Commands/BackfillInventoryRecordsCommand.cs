using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using Inv = OmniBlox.Domain.Entities.Inventory;

namespace OmniBlox.Application.Features.Inventory.Commands;

public record BackfillInventoryRecordsCommand : IRequest<BackfillResult>;

public record BackfillResult
{
    public int StockMovementsCreated { get; init; }
    public int ProductsCorrected { get; init; }
    public int ProductsSkipped { get; init; }
}

public class BackfillInventoryRecordsCommandHandler : IRequestHandler<BackfillInventoryRecordsCommand, BackfillResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public BackfillInventoryRecordsCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<BackfillResult> Handle(BackfillInventoryRecordsCommand request, CancellationToken ct)
    {
        int movementsCreated = 0;
        int productsCorrected = 0;
        int productsSkipped = 0;

        // Get all existing inventory records
        var inventories = await _context.Inventories
            .Include(i => i.Product)
            .ToListAsync(ct);

        // Get existing stock movement product+warehouse pairs to avoid duplicates
        var existingMovementPairs = await _context.StockMovements
            .Select(m => new { m.ProductId, m.WarehouseId })
            .Distinct()
            .ToListAsync(ct);

        var existingSet = existingMovementPairs
            .Select(x => (x.ProductId, x.WarehouseId))
            .ToHashSet();

        foreach (var inv in inventories)
        {
            if (existingSet.Contains((inv.ProductId, inv.WarehouseId)))
            {
                productsSkipped++;
                continue;
            }

            // Create opening stock movement for each existing inventory record
            _context.StockMovements.Add(new StockMovement
            {
                ProductId = inv.ProductId,
                WarehouseId = inv.WarehouseId,
                MovementType = MovementType.opening_stock,
                Quantity = inv.Quantity,
                ReferenceType = "backfill",
                BalanceAfter = inv.Quantity,
                CreatedBy = _currentUser.UserId,
            });
            movementsCreated++;
        }

        // Also create stock movements for products with Stock > 0 but no inventory record
        var productIdsWithInventory = inventories.Select(i => i.ProductId).ToHashSet();
        var productsWithoutInventory = await _context.Products
            .Where(p => !productIdsWithInventory.Contains(p.Id) && p.Stock > 0)
            .ToListAsync(ct);

        var defaultWarehouse = await _context.Warehouses
            .Where(w => w.CompanyId == _currentUser.CompanyId)
            .FirstOrDefaultAsync(ct);

        if (defaultWarehouse is not null)
        {
            foreach (var product in productsWithoutInventory)
            {
                var pair = (product.Id, defaultWarehouse.Id);
                if (existingSet.Contains(pair)) continue;

                _context.Inventories.Add(new Inv
                {
                    ProductId = product.Id,
                    WarehouseId = defaultWarehouse.Id,
                    Quantity = product.Stock,
                });

                _context.StockMovements.Add(new StockMovement
                {
                    ProductId = product.Id,
                    WarehouseId = defaultWarehouse.Id,
                    MovementType = MovementType.opening_stock,
                    Quantity = product.Stock,
                    ReferenceType = "backfill",
                    BalanceAfter = product.Stock,
                    CreatedBy = _currentUser.UserId,
                });
                movementsCreated++;
            }
        }

        // Reconcile Product.Stock against sum of Inventory.Quantity
        var allProducts = await _context.Products.ToListAsync(ct);
        var stockSums = await _context.Inventories
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Total = g.Sum(i => i.Quantity) })
            .ToDictionaryAsync(x => x.ProductId, x => x.Total, ct);

        foreach (var product in allProducts)
        {
            var expectedStock = stockSums.GetValueOrDefault(product.Id, 0);
            if (product.Stock != expectedStock)
            {
                product.Stock = expectedStock;
                product.UpdatedAt = DateTime.UtcNow;
                productsCorrected++;
            }
        }

        await _context.SaveChangesAsync(ct);

        return new BackfillResult
        {
            StockMovementsCreated = movementsCreated,
            ProductsCorrected = productsCorrected,
            ProductsSkipped = productsSkipped,
        };
    }
}
