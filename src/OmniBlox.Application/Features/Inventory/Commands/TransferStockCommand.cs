using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Inventory.DTOs;
using Inv = OmniBlox.Domain.Entities.Inventory;
using StockLedgerEntry = OmniBlox.Domain.Entities.StockLedgerEntry;

namespace OmniBlox.Application.Features.Inventory.Commands;

public record TransferStockCommand : IRequest<StockTransferDto>
{
    public Guid ProductId { get; init; }
    public Guid FromWarehouseId { get; init; }
    public Guid ToWarehouseId { get; init; }
    public int Quantity { get; init; }
    public string? Note { get; init; }
}

public class TransferStockCommandHandler : IRequestHandler<TransferStockCommand, StockTransferDto>
{
    private readonly IApplicationDbContext _context;
    public TransferStockCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<StockTransferDto> Handle(TransferStockCommand request, CancellationToken ct)
    {
        if (request.FromWarehouseId == request.ToWarehouseId)
            throw new InvalidOperationException("Source and destination warehouses must be different.");

        var fromInv = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == request.ProductId && i.WarehouseId == request.FromWarehouseId, ct);
        if (fromInv is null || fromInv.Quantity < request.Quantity)
            throw new InvalidOperationException("Insufficient stock in source warehouse.");

        var toInv = await _context.Inventories
            .FirstOrDefaultAsync(i => i.ProductId == request.ProductId && i.WarehouseId == request.ToWarehouseId, ct);

        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId, ct);
        if (product is null) throw new KeyNotFoundException("Product not found.");

        var fromWh = await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == request.FromWarehouseId, ct);
        var toWh = await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == request.ToWarehouseId, ct);

        fromInv.Quantity -= request.Quantity;
        fromInv.UpdatedAt = DateTime.UtcNow;

        if (toInv is null)
        {
            toInv = new Inv
            {
                ProductId = request.ProductId,
                WarehouseId = request.ToWarehouseId,
                Quantity = request.Quantity,
            };
            _context.Inventories.Add(toInv);
        }
        else
        {
            toInv.Quantity += request.Quantity;
            toInv.UpdatedAt = DateTime.UtcNow;
        }

        var totalQty = await _context.Inventories
            .Where(i => i.ProductId == request.ProductId)
            .SumAsync(i => (int?)i.Quantity ?? 0, ct);
        product.Stock = totalQty;
        product.UpdatedAt = DateTime.UtcNow;

        var note = request.Note ?? $"Transfer from {fromWh?.Name} to {toWh?.Name}";
        var outEntry = new StockLedgerEntry
        {
            ProductId = request.ProductId,
            WarehouseId = request.FromWarehouseId,
            Quantity = -request.Quantity,
            Balance = fromInv.Quantity,
            Type = "TRANSFER_OUT",
            Reference = note,
        };
        _context.StockLedgerEntries.Add(outEntry);
        var inEntry = new StockLedgerEntry
        {
            ProductId = request.ProductId,
            WarehouseId = request.ToWarehouseId,
            Quantity = request.Quantity,
            Balance = toInv.Quantity,
            Type = "TRANSFER_IN",
            Reference = note,
        };
        _context.StockLedgerEntries.Add(inEntry);

        await _context.SaveChangesAsync(ct);

        return new StockTransferDto
        {
            Id = outEntry.Id,
            ReferenceNumber = note,
            TotalItems = 1,
            AdjustmentDate = DateTime.UtcNow,
            Notes = request.Note,
            CreatedAt = DateTime.UtcNow,
            Items = new List<StockAdjustmentItemDto>
            {
                new()
                {
                    Id = outEntry.Id,
                    NewQuantity = request.Quantity,
                    Difference = -request.Quantity,
                    Product = new ItemProductInfo { Name = product.Name, Sku = product.SKU, ImageUrl = product.ImageUrl },
                    Warehouse = new ItemWarehouseInfo { Name = fromWh?.Name ?? "" },
                },
                new()
                {
                    Id = inEntry.Id,
                    NewQuantity = request.Quantity,
                    Difference = request.Quantity,
                    Product = new ItemProductInfo { Name = product.Name, Sku = product.SKU, ImageUrl = product.ImageUrl },
                    Warehouse = new ItemWarehouseInfo { Name = toWh?.Name ?? "" },
                },
            },
        };
    }
}

public class TransferStockCommandValidator : AbstractValidator<TransferStockCommand>
{
    public TransferStockCommandValidator()
    {
        RuleFor(v => v.ProductId).NotEmpty();
        RuleFor(v => v.FromWarehouseId).NotEmpty();
        RuleFor(v => v.ToWarehouseId).NotEmpty();
        RuleFor(v => v.Quantity).GreaterThan(0);
    }
}
