using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.PurchaseReturns.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.PurchaseReturns.Commands;

public record CreatePurchaseReturnCommand : IRequest<PurchaseReturnDetailDto>
{
    public Guid WarehouseId { get; init; }
    public Guid SupplierId { get; init; }
    public Guid? PurchaseOrderId { get; init; }
    public string? Reason { get; init; }
    public DateTime ReturnDate { get; init; }
    public List<CreatePurchaseReturnItem> Items { get; init; } = [];
}

public class CreatePurchaseReturnCommandHandler : IRequestHandler<CreatePurchaseReturnCommand, PurchaseReturnDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public CreatePurchaseReturnCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PurchaseReturnDetailDto> Handle(CreatePurchaseReturnCommand request, CancellationToken ct)
    {
        var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == request.WarehouseId, ct);
        if (warehouse is null) throw new NotFoundException(nameof(Warehouse), request.WarehouseId);

        var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == request.SupplierId, ct);
        if (supplier is null) throw new NotFoundException(nameof(Supplier), request.SupplierId);

        if (request.PurchaseOrderId.HasValue)
        {
            var po = await _context.PurchaseOrders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == request.PurchaseOrderId.Value, ct);
            if (po is null) throw new NotFoundException(nameof(PurchaseOrder), request.PurchaseOrderId.Value);

            foreach (var item in request.Items)
            {
                if (item.PurchaseOrderItemId.HasValue)
                {
                    var poi = po.Items.FirstOrDefault(i => i.Id == item.PurchaseOrderItemId.Value);
                    if (poi is null)
                        throw new NotFoundException(nameof(PurchaseOrderItem), item.PurchaseOrderItemId.Value);

                    if (poi.ReturnedQuantity + item.Quantity > poi.Quantity)
                        throw new InvalidOperationException(
                            $"Cannot return more than ordered quantity for product {item.ProductId}. Ordered: {poi.Quantity}, already returned: {poi.ReturnedQuantity}, attempting: {item.Quantity}");
                }
            }

            var stockCheck = request.Items.All(i =>
            {
                if (!i.PurchaseOrderItemId.HasValue) return true;
                var poi = po.Items.FirstOrDefault(p => p.Id == i.PurchaseOrderItemId.Value);
                return poi is null || poi.ReturnedQuantity + i.Quantity <= poi.Quantity;
            });
        }

        foreach (var item in request.Items)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId, ct);
            if (product is null) throw new NotFoundException(nameof(Product), item.ProductId);

            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.WarehouseId == request.WarehouseId, ct);

            if (inventory is null || inventory.Quantity < item.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for product {product.Name}. Available: {inventory?.Quantity ?? 0}, requested: {item.Quantity}");
        }

        var referenceNumber = $"PR-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{Random.Shared.Next(1000, 9999)}";
        var totalAmount = request.Items.Sum(i => i.Quantity * i.UnitCost);

        var returnEntity = new PurchaseReturn
        {
            UserId = _currentUser.UserId,
            WarehouseId = request.WarehouseId,
            SupplierId = request.SupplierId,
            PurchaseOrderId = request.PurchaseOrderId,
            Reason = request.Reason,
            ReturnDate = request.ReturnDate == default ? DateTime.UtcNow : request.ReturnDate,
            ReferenceNumber = referenceNumber,
            TotalAmount = totalAmount,
            Status = "PENDING",
        };

        _context.PurchaseReturns.Add(returnEntity);
        await _context.SaveChangesAsync(ct);

        foreach (var item in request.Items)
        {
            _context.PurchaseReturnItems.Add(new PurchaseReturnItem
            {
                PurchaseReturnId = returnEntity.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitCost = item.UnitCost,
                PurchaseOrderItemId = item.PurchaseOrderItemId,
            });
        }

        await _context.SaveChangesAsync(ct);

        var created = await _context.PurchaseReturns
            .Include(r => r.Warehouse)
            .Include(r => r.Supplier)
            .Include(r => r.PurchaseOrder)
            .Include(r => r.Items).ThenInclude(i => i.Product)
            .FirstAsync(r => r.Id == returnEntity.Id, ct);

        return MapToDetail(created);
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

public class CreatePurchaseReturnCommandValidator : AbstractValidator<CreatePurchaseReturnCommand>
{
    public CreatePurchaseReturnCommandValidator()
    {
        RuleFor(v => v.WarehouseId).NotEmpty();
        RuleFor(v => v.SupplierId).NotEmpty();
        RuleFor(v => v.ReturnDate).NotEmpty();
        RuleFor(v => v.Items).NotEmpty();
        RuleForEach(v => v.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitCost).GreaterThanOrEqualTo(0);
        });
    }
}
