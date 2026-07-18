using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.PurchaseReturns.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.PurchaseReturns.Commands;

public record UpdatePurchaseReturnCommand : IRequest<PurchaseReturnDetailDto>
{
    public Guid Id { get; init; }
    public Guid WarehouseId { get; init; }
    public Guid SupplierId { get; init; }
    public Guid? PurchaseOrderId { get; init; }
    public string? Reason { get; init; }
    public DateTime ReturnDate { get; init; }
    public List<CreatePurchaseReturnItem> Items { get; init; } = [];
}

public class UpdatePurchaseReturnCommandHandler : IRequestHandler<UpdatePurchaseReturnCommand, PurchaseReturnDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdatePurchaseReturnCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PurchaseReturnDetailDto> Handle(UpdatePurchaseReturnCommand request, CancellationToken ct)
    {
        var returnEntity = await _context.PurchaseReturns
            .Include(r => r.Items)
            .Include(r => r.Warehouse)
            .Include(r => r.Supplier)
            .Include(r => r.PurchaseOrder)
            .AsTracking().FirstOrDefaultAsync(r => r.Id == request.Id, ct);
        if (returnEntity is null) throw new NotFoundException(nameof(PurchaseReturn), request.Id);

        var warehouse = await _context.Warehouses.AsTracking().FirstOrDefaultAsync(w => w.Id == request.WarehouseId, ct);
        if (warehouse is null) throw new NotFoundException(nameof(Warehouse), request.WarehouseId);

        var supplier = await _context.Suppliers.AsTracking().FirstOrDefaultAsync(s => s.Id == request.SupplierId, ct);
        if (supplier is null) throw new NotFoundException(nameof(Supplier), request.SupplierId);

        if (request.PurchaseOrderId.HasValue)
        {
            var po = await _context.PurchaseOrders
                .Include(o => o.Items)
                .AsTracking().FirstOrDefaultAsync(o => o.Id == request.PurchaseOrderId.Value, ct);
            if (po is null) throw new NotFoundException(nameof(PurchaseOrder), request.PurchaseOrderId.Value);
        }

        foreach (var item in request.Items)
        {
            var product = await _context.Products.AsTracking().FirstOrDefaultAsync(p => p.Id == item.ProductId, ct);
            if (product is null) throw new NotFoundException(nameof(Product), item.ProductId);
        }

        returnEntity.WarehouseId = request.WarehouseId;
        returnEntity.SupplierId = request.SupplierId;
        returnEntity.PurchaseOrderId = request.PurchaseOrderId;
        returnEntity.Reason = request.Reason;
        returnEntity.ReturnDate = request.ReturnDate;

        _context.PurchaseReturnItems.RemoveRange(returnEntity.Items);

        var totalAmount = request.Items.Sum(i => i.Quantity * i.UnitCost);
        returnEntity.TotalAmount = totalAmount;

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

        returnEntity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        var updated = await _context.PurchaseReturns
            .Include(r => r.Warehouse)
            .Include(r => r.Supplier)
            .Include(r => r.PurchaseOrder)
            .Include(r => r.Items).ThenInclude(i => i.Product)
            .AsTracking().FirstAsync(r => r.Id == returnEntity.Id, ct);

        return MapToDetail(updated);
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

public class UpdatePurchaseReturnCommandValidator : AbstractValidator<UpdatePurchaseReturnCommand>
{
    public UpdatePurchaseReturnCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
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
