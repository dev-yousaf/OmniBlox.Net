using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.PurchaseReturns.Commands;

public record DeletePurchaseReturnCommand : IRequest
{
    public Guid Id { get; init; }
}

public class DeletePurchaseReturnCommandHandler : IRequestHandler<DeletePurchaseReturnCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IStockService _stockService;

    public DeletePurchaseReturnCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IStockService stockService)
    {
        _context = context;
        _currentUser = currentUser;
        _stockService = stockService;
    }

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
                await _stockService.RecordMovementAsync(new RecordMovementArgs
                {
                    ProductId = item.ProductId,
                    WarehouseId = returnEntity.WarehouseId,
                    MovementType = MovementType.purchase,
                    Quantity = item.Quantity,
                    ReferenceType = "purchase_return",
                    ReferenceId = returnEntity.Id,
                    UserId = _currentUser.UserId,
                }, ct);

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
