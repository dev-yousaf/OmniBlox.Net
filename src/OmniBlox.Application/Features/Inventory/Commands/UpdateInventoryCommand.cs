using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Inventory.DTOs;
using Inv = OmniBlox.Domain.Entities.Inventory;
using StockLedgerEntry = OmniBlox.Domain.Entities.StockLedgerEntry;

namespace OmniBlox.Application.Features.Inventory.Commands;

public record UpdateInventoryCommand : IRequest<InventoryDto>
{
    public Guid ProductId { get; init; }
    public Guid WarehouseId { get; init; }
    public int Quantity { get; init; }
    public string? Notes { get; init; }
}

public class UpdateInventoryCommandHandler : IRequestHandler<UpdateInventoryCommand, InventoryDto>
{
    private readonly IApplicationDbContext _context;
    public UpdateInventoryCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<InventoryDto> Handle(UpdateInventoryCommand request, CancellationToken ct)
    {
        var inventory = await _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .FirstOrDefaultAsync(i => i.ProductId == request.ProductId && i.WarehouseId == request.WarehouseId, ct);

        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId, ct);
        if (product is null) throw new KeyNotFoundException("Product not found.");

        var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == request.WarehouseId, ct);
        if (warehouse is null) throw new KeyNotFoundException("Warehouse not found.");

        int previousQty = inventory?.Quantity ?? 0;
        int difference = request.Quantity - previousQty;

        if (inventory is null)
        {
            inventory = new Inv
            {
                ProductId = request.ProductId,
                WarehouseId = request.WarehouseId,
                Quantity = request.Quantity,
            };
            _context.Inventories.Add(inventory);
        }
        else
        {
            inventory.Quantity = request.Quantity;
            inventory.UpdatedAt = DateTime.UtcNow;
        }

        product.Stock += difference;

        var noteText = string.IsNullOrWhiteSpace(request.Notes)
            ? $"Inventory update at {warehouse.Name}"
            : request.Notes;

        _context.StockLedgerEntries.Add(new StockLedgerEntry
        {
            ProductId = request.ProductId,
            WarehouseId = request.WarehouseId,
            Quantity = difference,
            Balance = request.Quantity,
            Type = "INVENTORY_UPDATE",
            Reference = $"Inventory update at {warehouse.Name}",
            Note = noteText,
        });

        await _context.SaveChangesAsync(ct);
        return InventoryDto.FromEntity(inventory);
    }
}

public class UpdateInventoryCommandValidator : AbstractValidator<UpdateInventoryCommand>
{
    public UpdateInventoryCommandValidator()
    {
        RuleFor(v => v.ProductId).NotEmpty();
        RuleFor(v => v.WarehouseId).NotEmpty();
        RuleFor(v => v.Quantity).GreaterThanOrEqualTo(0);
    }
}
