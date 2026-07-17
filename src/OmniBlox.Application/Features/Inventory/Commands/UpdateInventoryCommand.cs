using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Inventory.DTOs;
using OmniBlox.Domain.Enums;

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
    private readonly ICurrentUserService _currentUser;
    private readonly IStockService _stockService;

    public UpdateInventoryCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IStockService stockService)
    {
        _context = context;
        _currentUser = currentUser;
        _stockService = stockService;
    }

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

        if (difference != 0)
        {
            var movementType = difference > 0 ? MovementType.adjustment_in : MovementType.adjustment_out;

            await _stockService.RecordMovementAsync(new RecordMovementArgs
            {
                ProductId = request.ProductId,
                WarehouseId = request.WarehouseId,
                MovementType = movementType,
                Quantity = Math.Abs(difference),
                ReferenceType = "inventory_update",
                ReferenceId = null,
                UserId = _currentUser.UserId,
            }, ct);
        }

        await _context.SaveChangesAsync(ct);

        var updatedInventory = await _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .FirstAsync(i => i.ProductId == request.ProductId && i.WarehouseId == request.WarehouseId, ct);

        return InventoryDto.FromEntity(updatedInventory);
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
