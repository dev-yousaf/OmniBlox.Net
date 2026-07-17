using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;

namespace OmniBlox.Infrastructure.Services;

public class StockService : IStockService
{
    private readonly IApplicationDbContext _context;

    public StockService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StockMovement> RecordMovementAsync(RecordMovementArgs args, CancellationToken ct = default)
    {
        var delta = GetDelta(args.MovementType, args.Quantity);

        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == args.ProductId && i.WarehouseId == args.WarehouseId, ct);

        int oldQty;
        if (inventory is null)
        {
            if (delta < 0)
                throw new InvalidOperationException(
                    $"Cannot reduce stock below zero. No existing inventory record for product {args.ProductId} in warehouse {args.WarehouseId}.");
            oldQty = 0;
            inventory = new Inventory
            {
                ProductId = args.ProductId,
                WarehouseId = args.WarehouseId,
                Quantity = delta,
            };
            _context.Inventories.Add(inventory);
        }
        else
        {
            oldQty = inventory.Quantity;
            var newQty = oldQty + delta;
            if (newQty < 0)
                throw new InvalidOperationException(
                    $"Insufficient stock. Product {args.ProductId} in warehouse {args.WarehouseId} has {oldQty}, attempted change of {delta}.");
            inventory.Quantity = newQty;
            inventory.UpdatedAt = DateTime.UtcNow;
        }

        var movement = new StockMovement
        {
            ProductId = args.ProductId,
            WarehouseId = args.WarehouseId,
            MovementType = args.MovementType,
            Quantity = args.Quantity,
            ReferenceType = args.ReferenceType,
            ReferenceId = args.ReferenceId,
            BalanceAfter = inventory.Quantity,
            CreatedBy = args.UserId,
        };
        _context.StockMovements.Add(movement);

        // Update product total stock in lockstep — delta is the net change for this product+warehouse
        var product = await _context.Products.FirstAsync(p => p.Id == args.ProductId, ct);
        product.Stock += delta;
        product.UpdatedAt = DateTime.UtcNow;

        return movement;
    }

    public async Task<(StockMovement OutMovement, StockMovement InMovement)> RecordTransferAsync(
        RecordTransferArgs args, CancellationToken ct = default)
    {
        // Transfer out: reduces source warehouse, does NOT affect product.Stock yet
        var outMovement = await RecordMovementCoreAsync(
            args.ProductId, args.FromWarehouseId, MovementType.transfer_out,
            -args.Quantity, args.ReferenceType, args.ReferenceId, args.UserId, skipProductUpdate: true, ct);

        // Transfer in: increases destination warehouse, does NOT affect product.Stock yet
        var inMovement = await RecordMovementCoreAsync(
            args.ProductId, args.ToWarehouseId, MovementType.transfer_in,
            args.Quantity, args.ReferenceType, args.ReferenceId, args.UserId, skipProductUpdate: true, ct);

        // Net effect on product.Stock is zero — recalculate to ensure correctness
        var dbTotal = await _context.Inventories
            .Where(i => i.ProductId == args.ProductId)
            .SumAsync(i => (int?)i.Quantity ?? 0, ct);
        var product = await _context.Products.FirstAsync(p => p.Id == args.ProductId, ct);
        product.Stock = dbTotal;
        product.UpdatedAt = DateTime.UtcNow;

        return (outMovement, inMovement);
    }

    private async Task<StockMovement> RecordMovementCoreAsync(
        Guid productId, Guid warehouseId, MovementType movementType,
        int delta, string? referenceType, Guid? referenceId, Guid userId,
        bool skipProductUpdate, CancellationToken ct)
    {
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == productId && i.WarehouseId == warehouseId, ct);

        if (inventory is null)
        {
            if (delta < 0)
                throw new InvalidOperationException(
                    $"Cannot reduce stock below zero. No existing inventory record for product {productId} in warehouse {warehouseId}.");

            inventory = new Inventory
            {
                ProductId = productId,
                WarehouseId = warehouseId,
                Quantity = delta,
            };
            _context.Inventories.Add(inventory);
        }
        else
        {
            var newQty = inventory.Quantity + delta;
            if (newQty < 0)
                throw new InvalidOperationException(
                    $"Insufficient stock. Product {productId} in warehouse {warehouseId} has {inventory.Quantity}, attempted change of {delta}.");
            inventory.Quantity = newQty;
            inventory.UpdatedAt = DateTime.UtcNow;
        }

        var movement = new StockMovement
        {
            ProductId = productId,
            WarehouseId = warehouseId,
            MovementType = movementType,
            Quantity = Math.Abs(delta),
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            BalanceAfter = inventory.Quantity,
            CreatedBy = userId,
        };
        _context.StockMovements.Add(movement);

        if (!skipProductUpdate)
        {
            var product = await _context.Products.FirstAsync(p => p.Id == productId, ct);
            product.Stock += delta;
            product.UpdatedAt = DateTime.UtcNow;
        }

        return movement;
    }

    private static int GetDelta(MovementType type, int quantity)
    {
        return type switch
        {
            MovementType.opening_stock => quantity,
            MovementType.purchase => quantity,
            MovementType.sale_return => quantity,
            MovementType.adjustment_in => quantity,
            MovementType.transfer_in => quantity,
            MovementType.purchase_return => -quantity,
            MovementType.sale => -quantity,
            MovementType.adjustment_out => -quantity,
            MovementType.transfer_out => -quantity,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }
}
