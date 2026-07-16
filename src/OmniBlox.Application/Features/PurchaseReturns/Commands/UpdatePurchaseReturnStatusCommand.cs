using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.PurchaseReturns.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;
using Inv = OmniBlox.Domain.Entities.Inventory;

namespace OmniBlox.Application.Features.PurchaseReturns.Commands;

public record UpdatePurchaseReturnStatusCommand : IRequest<PurchaseReturnDetailDto>
{
    public Guid Id { get; init; }
    public string Status { get; init; } = "COMPLETED";
}

public class UpdatePurchaseReturnStatusCommandHandler : IRequestHandler<UpdatePurchaseReturnStatusCommand, PurchaseReturnDetailDto>
{
    private readonly IApplicationDbContext _context;
    public UpdatePurchaseReturnStatusCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<PurchaseReturnDetailDto> Handle(UpdatePurchaseReturnStatusCommand request, CancellationToken ct)
    {
        var returnEntity = await _context.PurchaseReturns
            .Include(r => r.Warehouse)
            .Include(r => r.Supplier)
            .Include(r => r.PurchaseOrder)
            .Include(r => r.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(r => r.Id == request.Id, ct);
        if (returnEntity is null) throw new NotFoundException(nameof(PurchaseReturn), request.Id);

        var oldStatus = returnEntity.Status;

        if (oldStatus == "COMPLETED" && request.Status == "CANCELLED")
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
                        Type = "PURCHASE_RETURN_REVERSAL",
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

            var po = returnEntity.PurchaseOrder;
            if (po is not null)
            {
                var hasReturns = await _context.PurchaseReturnItems
                    .Where(ri => ri.PurchaseReturn.PurchaseOrderId == po.Id && ri.PurchaseReturn.Status == "COMPLETED")
                    .AnyAsync(ct);
                po.HasReturns = hasReturns;
            }
        }
        else if (oldStatus == "PENDING" && request.Status == "COMPLETED")
        {
            foreach (var item in returnEntity.Items)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.WarehouseId == returnEntity.WarehouseId, ct);

                var availableQty = inventory?.Quantity ?? 0;
                if (item.Quantity > availableQty)
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock for product '{item.Product?.Name}'. Available: {availableQty}, requested: {item.Quantity}.");
                }

                if (inventory is null)
                {
                    inventory = new Inv
                    {
                        ProductId = item.ProductId,
                        WarehouseId = returnEntity.WarehouseId,
                        Quantity = -item.Quantity,
                    };
                    _context.Inventories.Add(inventory);
                }
                else
                {
                    inventory.Quantity -= item.Quantity;
                    inventory.UpdatedAt = DateTime.UtcNow;
                }

                _context.StockLedgerEntries.Add(new StockLedgerEntry
                {
                    ProductId = item.ProductId,
                    WarehouseId = returnEntity.WarehouseId,
                    Quantity = -item.Quantity,
                    Balance = inventory.Quantity,
                    Type = "PURCHASE_RETURN",
                    Reference = returnEntity.ReferenceNumber,
                });

                if (item.Product is not null)
                {
                    item.Product.Stock -= item.Quantity;
                }

                if (item.PurchaseOrderItemId.HasValue)
                {
                    var poi = await _context.PurchaseOrderItems
                        .FirstOrDefaultAsync(i => i.Id == item.PurchaseOrderItemId.Value, ct);
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
