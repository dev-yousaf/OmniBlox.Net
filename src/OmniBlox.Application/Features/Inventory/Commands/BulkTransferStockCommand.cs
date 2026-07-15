using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Inventory.DTOs;
using Inv = OmniBlox.Domain.Entities.Inventory;
using StockLedgerEntry = OmniBlox.Domain.Entities.StockLedgerEntry;

namespace OmniBlox.Application.Features.Inventory.Commands;

public record BulkTransferStockCommand : IRequest<StockTransferDto>
{
    public Guid FromWarehouseId { get; init; }
    public Guid ToWarehouseId { get; init; }
    public List<BulkTransferItem> Items { get; init; } = [];
    public string? Note { get; init; }
}

public class BulkTransferStockCommandHandler : IRequestHandler<BulkTransferStockCommand, StockTransferDto>
{
    private readonly IApplicationDbContext _context;
    public BulkTransferStockCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<StockTransferDto> Handle(BulkTransferStockCommand request, CancellationToken ct)
    {
        if (request.FromWarehouseId == request.ToWarehouseId)
            throw new InvalidOperationException("Source and destination warehouses must be different.");

        var fromWh = await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == request.FromWarehouseId, ct);
        var toWh = await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == request.ToWarehouseId, ct);

        var note = request.Note ?? $"Bulk transfer from {fromWh?.Name} to {toWh?.Name}";
        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync(ct);
        var productMap = products.ToDictionary(p => p.Id);

        foreach (var item in request.Items)
        {
            var fromInv = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.WarehouseId == request.FromWarehouseId, ct);
            if (fromInv is null || fromInv.Quantity < item.Quantity)
            {
                var p = productMap.GetValueOrDefault(item.ProductId);
                throw new InvalidOperationException($"Insufficient stock for product '{p?.Name ?? item.ProductId.ToString()}' in {fromWh?.Name}.");
            }

            var toInv = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.WarehouseId == request.ToWarehouseId, ct);

            fromInv.Quantity -= item.Quantity;
            fromInv.UpdatedAt = DateTime.UtcNow;

            if (toInv is null)
            {
                toInv = new Inv { ProductId = item.ProductId, WarehouseId = request.ToWarehouseId, Quantity = item.Quantity };
                _context.Inventories.Add(toInv);
            }
            else
            {
                toInv.Quantity += item.Quantity;
                toInv.UpdatedAt = DateTime.UtcNow;
            }

            _context.StockLedgerEntries.Add(new StockLedgerEntry
            {
                ProductId = item.ProductId, WarehouseId = request.FromWarehouseId,
                Quantity = -item.Quantity, Balance = fromInv.Quantity,
                Type = "TRANSFER_OUT", Reference = note,
            });
            _context.StockLedgerEntries.Add(new StockLedgerEntry
            {
                ProductId = item.ProductId, WarehouseId = request.ToWarehouseId,
                Quantity = item.Quantity, Balance = toInv.Quantity,
                Type = "TRANSFER_IN", Reference = note,
            });

            if (productMap.TryGetValue(item.ProductId, out var prod))
            {
                var totalQty = await _context.Inventories
                    .Where(i => i.ProductId == item.ProductId)
                    .SumAsync(i => (int?)i.Quantity ?? 0, ct);
                prod.Stock = totalQty;
                prod.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(ct);

        var firstEntry = await _context.StockLedgerEntries
            .Include(l => l.Product)
            .Include(l => l.Warehouse)
            .Where(l => l.Reference == note && l.Type == "TRANSFER_OUT")
            .ToListAsync(ct);

        var items = firstEntry.Select(e => new StockAdjustmentItemDto
        {
            Id = e.Id,
            NewQuantity = Math.Abs(e.Quantity),
            Difference = e.Quantity,
            Product = new ItemProductInfo
            {
                Name = e.Product?.Name ?? "",
                Sku = e.Product?.SKU ?? "",
                ImageUrl = e.Product?.ImageUrl,
            },
            Warehouse = new ItemWarehouseInfo
            {
                Name = fromWh?.Name ?? "",
            },
        }).ToList();

        var inEntries = await _context.StockLedgerEntries
            .Include(l => l.Product)
            .Include(l => l.Warehouse)
            .Where(l => l.Reference == note && l.Type == "TRANSFER_IN")
            .ToListAsync(ct);

        items.AddRange(inEntries.Select(e => new StockAdjustmentItemDto
        {
            Id = e.Id,
            NewQuantity = Math.Abs(e.Quantity),
            Difference = e.Quantity,
            Product = new ItemProductInfo
            {
                Name = e.Product?.Name ?? "",
                Sku = e.Product?.SKU ?? "",
                ImageUrl = e.Product?.ImageUrl,
            },
            Warehouse = new ItemWarehouseInfo
            {
                Name = toWh?.Name ?? "",
            },
        }));

        return new StockTransferDto
        {
            Id = firstEntry.FirstOrDefault()?.Id ?? Guid.Empty,
            ReferenceNumber = note,
            TotalItems = request.Items.Count,
            AdjustmentDate = DateTime.UtcNow,
            Notes = request.Note,
            CreatedAt = DateTime.UtcNow,
            Items = items,
        };
    }
}

public class BulkTransferStockCommandValidator : AbstractValidator<BulkTransferStockCommand>
{
    public BulkTransferStockCommandValidator()
    {
        RuleFor(v => v.FromWarehouseId).NotEmpty();
        RuleFor(v => v.ToWarehouseId).NotEmpty();
        RuleFor(v => v.Items).NotEmpty();
    }
}
