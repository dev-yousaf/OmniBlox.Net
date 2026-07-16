using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;
using Inv = OmniBlox.Domain.Entities.Inventory;

namespace OmniBlox.Application.Features.Purchases.Commands;

public record DeletePurchaseOrderCommand : IRequest
{
    public Guid Id { get; init; }
}

public class DeletePurchaseOrderCommandHandler : IRequestHandler<DeletePurchaseOrderCommand>
{
    private readonly IApplicationDbContext _context;
    public DeletePurchaseOrderCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(DeletePurchaseOrderCommand request, CancellationToken ct)
    {
        var order = await _context.PurchaseOrders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == request.Id, ct);
        if (order is null) throw new NotFoundException(nameof(PurchaseOrder), request.Id);

        if (order.Status == "COMPLETED" && order.WarehouseId.HasValue)
        {
            foreach (var item in order.Items)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.WarehouseId == order.WarehouseId.Value, ct);

                if (inventory is not null)
                {
                    inventory.Quantity -= item.Quantity;
                    inventory.UpdatedAt = DateTime.UtcNow;

                    if (inventory.Quantity < 0) inventory.Quantity = 0;

                    _context.StockLedgerEntries.Add(new StockLedgerEntry
                    {
                        ProductId = item.ProductId,
                        WarehouseId = order.WarehouseId.Value,
                        Quantity = -item.Quantity,
                        Balance = inventory.Quantity,
                        Type = "PURCHASE_DELETE",
                        Reference = order.ReferenceNumber,
                    });

                    if (item.Product is not null)
                    {
                        item.Product.Stock -= item.Quantity;
                    }
                }
            }
        }

        _context.PurchaseOrderItems.RemoveRange(order.Items);
        _context.PurchaseOrders.Remove(order);
        await _context.SaveChangesAsync(ct);
    }
}

public class DeletePurchaseOrderCommandValidator : AbstractValidator<DeletePurchaseOrderCommand>
{
    public DeletePurchaseOrderCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
    }
}
