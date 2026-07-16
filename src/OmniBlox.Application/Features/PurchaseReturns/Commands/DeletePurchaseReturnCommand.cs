using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.PurchaseReturns.Commands;

public record DeletePurchaseReturnCommand : IRequest
{
    public Guid Id { get; init; }
}

public class DeletePurchaseReturnCommandHandler : IRequestHandler<DeletePurchaseReturnCommand>
{
    private readonly IApplicationDbContext _context;
    public DeletePurchaseReturnCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(DeletePurchaseReturnCommand request, CancellationToken ct)
    {
        var returnEntity = await _context.PurchaseReturns
            .Include(r => r.Items).ThenInclude(i => i.Product)
            .Include(r => r.PurchaseOrder)
            .FirstOrDefaultAsync(r => r.Id == request.Id, ct);
        if (returnEntity is null) throw new NotFoundException(nameof(PurchaseReturn), request.Id);

        if (returnEntity.Status == "COMPLETED")
        {
            foreach (var item in returnEntity.Items)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.WarehouseId == returnEntity.WarehouseId, ct);

                if (inventory is not null)
                {
                    inventory.Quantity += item.Quantity;
                    inventory.UpdatedAt = DateTime.UtcNow;

                    _context.StockLedgerEntries.Add(new StockLedgerEntry
                    {
                        ProductId = item.ProductId,
                        WarehouseId = returnEntity.WarehouseId,
                        Quantity = item.Quantity,
                        Balance = inventory.Quantity,
                        Type = "PURCHASE_RETURN_DELETE",
                        Reference = returnEntity.ReferenceNumber,
                    });

                    if (item.Product is not null)
                    {
                        item.Product.Stock += item.Quantity;
                    }
                }

                if (item.PurchaseOrderItemId.HasValue)
                {
                    var poi = await _context.PurchaseOrderItems
                        .FirstOrDefaultAsync(i => i.Id == item.PurchaseOrderItemId.Value, ct);
                    if (poi is not null)
                    {
                        poi.ReturnedQuantity -= item.Quantity;
                        if (poi.ReturnedQuantity < 0) poi.ReturnedQuantity = 0;
                    }
                }
            }

            if (returnEntity.PurchaseOrder is not null)
            {
                var hasReturns = await _context.PurchaseReturnItems
                    .Where(ri => ri.PurchaseReturn.PurchaseOrderId == returnEntity.PurchaseOrder.Id
                        && ri.PurchaseReturn.Status == "COMPLETED"
                        && ri.PurchaseReturn.Id != returnEntity.Id)
                    .AnyAsync(ct);
                returnEntity.PurchaseOrder.HasReturns = hasReturns;
            }
        }

        _context.PurchaseReturnItems.RemoveRange(returnEntity.Items);
        _context.PurchaseReturns.Remove(returnEntity);
        await _context.SaveChangesAsync(ct);
    }
}

public class DeletePurchaseReturnCommandValidator : AbstractValidator<DeletePurchaseReturnCommand>
{
    public DeletePurchaseReturnCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
    }
}
