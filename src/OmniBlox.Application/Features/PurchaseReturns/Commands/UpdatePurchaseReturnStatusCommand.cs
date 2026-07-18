using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.PurchaseReturns.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.PurchaseReturns.Commands;

public record UpdatePurchaseReturnStatusCommand : IRequest<PurchaseReturnDetailDto>
{
    public Guid Id { get; init; }
    public string Status { get; init; } = "COMPLETED";
}

public class UpdatePurchaseReturnStatusCommandHandler : IRequestHandler<UpdatePurchaseReturnStatusCommand, PurchaseReturnDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IStockService _stockService;

    public UpdatePurchaseReturnStatusCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IStockService stockService)
    {
        _context = context;
        _currentUser = currentUser;
        _stockService = stockService;
    }

    public async Task<PurchaseReturnDetailDto> Handle(UpdatePurchaseReturnStatusCommand request, CancellationToken ct)
    {
        var returnEntity = await _context.PurchaseReturns
            .Include(r => r.Warehouse)
            .Include(r => r.Supplier)
            .Include(r => r.PurchaseOrder)
            .Include(r => r.Items).ThenInclude(i => i.Product)
            .AsTracking().FirstOrDefaultAsync(r => r.Id == request.Id, ct);
        if (returnEntity is null) throw new NotFoundException(nameof(PurchaseReturn), request.Id);

        var oldStatus = returnEntity.Status;

        if (oldStatus == "COMPLETED" && request.Status == "CANCELLED")
        {
            // Reverse the return: restore stock back
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
                        .AsTracking().FirstOrDefaultAsync(i => i.Id == item.PurchaseOrderItemId.Value, ct);
                    if (poi is not null)
                    {
                        poi.ReturnedQuantity -= item.Quantity;
                        if (poi.ReturnedQuantity < 0) poi.ReturnedQuantity = 0;
                    }
                }
            }

            var po = returnEntity.PurchaseOrder;
            if (po is not null)
            {
                var hasReturns = await _context.PurchaseReturnItems
                    .Where(ri => ri.PurchaseReturn.PurchaseOrderId == po.Id && ri.PurchaseReturn.Status == "COMPLETED")
                    .AnyAsync(ct);
                po.HasReturns = hasReturns;
            }
        }
        else if ((oldStatus == "PENDING" || oldStatus == "PROCESSING") && request.Status == "COMPLETED")
        {
            // Complete the return: remove stock
            foreach (var item in returnEntity.Items)
            {
                var inventory = await _context.Inventories
                    .AsTracking().FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.WarehouseId == returnEntity.WarehouseId, ct);

                var availableQty = inventory?.Quantity ?? 0;
                if (item.Quantity > availableQty)
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock for product '{item.Product?.Name}'. Available: {availableQty}, requested: {item.Quantity}.");
                }

                await _stockService.RecordMovementAsync(new RecordMovementArgs
                {
                    ProductId = item.ProductId,
                    WarehouseId = returnEntity.WarehouseId,
                    MovementType = MovementType.purchase_return,
                    Quantity = item.Quantity,
                    ReferenceType = "purchase_return",
                    ReferenceId = returnEntity.Id,
                    UserId = _currentUser.UserId,
                }, ct);

                if (item.PurchaseOrderItemId.HasValue)
                {
                    var poi = await _context.PurchaseOrderItems
                        .AsTracking().FirstOrDefaultAsync(i => i.Id == item.PurchaseOrderItemId.Value, ct);
                    if (poi is not null)
                    {
                        poi.ReturnedQuantity += item.Quantity;
                    }
                }
            }

            if (returnEntity.PurchaseOrder is not null)
            {
                returnEntity.PurchaseOrder.HasReturns = true;
            }
        }

        returnEntity.Status = request.Status;
        returnEntity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return MapToDetail(returnEntity);
    }

    private static PurchaseReturnDetailDto MapToDetail(PurchaseReturn returnEntity)
    {
        return new PurchaseReturnDetailDto
        {
            Id = returnEntity.Id,
            ReferenceNumber = returnEntity.ReferenceNumber,
            TotalAmount = returnEntity.TotalAmount,
            Reason = returnEntity.Reason,
            Status = returnEntity.Status,
            ReturnDate = returnEntity.ReturnDate,
            WarehouseId = returnEntity.WarehouseId,
            WarehouseName = returnEntity.Warehouse?.Name ?? "",
            SupplierId = returnEntity.SupplierId,
            SupplierName = returnEntity.Supplier?.Name ?? "",
            PurchaseOrderId = returnEntity.PurchaseOrderId,
            PurchaseOrderReference = returnEntity.PurchaseOrder?.ReferenceNumber,
            CreatedAt = returnEntity.CreatedAt,
            Items = returnEntity.Items.Select(i => new PurchaseReturnItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product?.Name,
                ProductSku = i.Product?.SKU,
                Quantity = i.Quantity,
                UnitCost = i.UnitCost,
                Total = i.Quantity * i.UnitCost,
                PurchaseOrderItemId = i.PurchaseOrderItemId,
            }).ToList(),
        };
    }
}

public class UpdatePurchaseReturnStatusCommandValidator : AbstractValidator<UpdatePurchaseReturnStatusCommand>
{
    public UpdatePurchaseReturnStatusCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.Status).NotEmpty().Must(s =>
            s == "PENDING" || s == "PROCESSING" || s == "COMPLETED" || s == "CANCELLED");
    }
}
