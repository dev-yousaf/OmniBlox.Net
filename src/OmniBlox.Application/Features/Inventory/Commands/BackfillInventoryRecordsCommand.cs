using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using Inv = OmniBlox.Domain.Entities.Inventory;

namespace OmniBlox.Application.Features.Inventory.Commands;

public record BackfillInventoryRecordsCommand : IRequest<BackfillResult>;

public record BackfillResult
{
    public int Created { get; init; }
    public int Skipped { get; init; }
}

public class BackfillInventoryRecordsCommandHandler : IRequestHandler<BackfillInventoryRecordsCommand, BackfillResult>
{
    private readonly IApplicationDbContext _context;
    public BackfillInventoryRecordsCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<BackfillResult> Handle(BackfillInventoryRecordsCommand request, CancellationToken ct)
    {
        var warehouses = await _context.Warehouses.OrderBy(w => w.Name).ToListAsync(ct);
        if (warehouses.Count == 0)
            return new BackfillResult { Created = 0, Skipped = 0 };

        var defaultWarehouse = warehouses.First();

        var products = await _context.Products
            .Where(p => p.Stock > 0)
            .ToListAsync(ct);

        var existingProductIds = await _context.Inventories
            .Select(i => i.ProductId)
            .Distinct()
            .ToListAsync(ct);

        var existingSet = existingProductIds.ToHashSet();

        int created = 0;

        foreach (var product in products)
        {
            if (existingSet.Contains(product.Id))
                continue;

            _context.Inventories.Add(new Inv
            {
                ProductId = product.Id,
                WarehouseId = defaultWarehouse.Id,
                Quantity = product.Stock,
            });

            created++;
        }

        await _context.SaveChangesAsync(ct);

        return new BackfillResult
        {
            Created = created,
            Skipped = existingSet.Count,
        };
    }
}
