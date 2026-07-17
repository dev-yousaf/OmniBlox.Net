using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Purchases.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Purchases.Commands;

public record UpdatePurchaseOrderCommand : IRequest<PurchaseOrderDetailDto>
{
    public Guid Id { get; init; }
    public Guid SupplierId { get; init; }
    public DateTime OrderDate { get; init; }
    public string? ReferenceNumber { get; init; }
    public string? BillNumber { get; init; }
    public DateTime? BillDate { get; init; }
    public DateTime? DueDate { get; init; }
    public string? PaymentStatus { get; init; }
    public string? PaymentMethod { get; init; }
    public string? Status { get; init; }
    public string? Notes { get; init; }
    public Guid? WarehouseId { get; init; }
    public List<CreatePurchaseItem> Items { get; init; } = [];
}

public class UpdatePurchaseOrderCommandHandler : IRequestHandler<UpdatePurchaseOrderCommand, PurchaseOrderDetailDto>
{
    private readonly IApplicationDbContext _context;
    public UpdatePurchaseOrderCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<PurchaseOrderDetailDto> Handle(UpdatePurchaseOrderCommand request, CancellationToken ct)
    {
        var order = await _context.PurchaseOrders
            .Include(o => o.Items)
            .Include(o => o.Supplier)
            .Include(o => o.Warehouse)
            .FirstOrDefaultAsync(o => o.Id == request.Id, ct);
        if (order is null) throw new NotFoundException(nameof(PurchaseOrder), request.Id);

        var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == request.SupplierId, ct);
        if (supplier is null) throw new NotFoundException(nameof(Supplier), request.SupplierId);

        if (request.WarehouseId.HasValue)
        {
            var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == request.WarehouseId.Value, ct);
            if (warehouse is null) throw new NotFoundException(nameof(Warehouse), request.WarehouseId.Value);
        }

        var totalAmount = request.Items.Sum(i => i.Quantity * i.UnitCost);

        order.SupplierId = request.SupplierId;
        order.OrderDate = request.OrderDate;
        order.ReferenceNumber = request.ReferenceNumber ?? order.ReferenceNumber;
        order.BillNumber = request.BillNumber;
        order.BillDate = request.BillDate;
        order.DueDate = request.DueDate;
        order.PaymentStatus = request.PaymentStatus ?? order.PaymentStatus;
        order.PaymentMethod = request.PaymentMethod;
        order.Status = request.Status ?? order.Status;
        order.Notes = request.Notes;
        order.WarehouseId = request.WarehouseId;
        order.TotalAmount = totalAmount;
        order.UpdatedAt = DateTime.UtcNow;

        _context.PurchaseOrderItems.RemoveRange(order.Items);

        foreach (var item in request.Items)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId, ct);
            if (product is null) throw new NotFoundException(nameof(Product), item.ProductId);

            _context.PurchaseOrderItems.Add(new PurchaseOrderItem
            {
                PurchaseOrderId = order.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitCost = item.UnitCost,
            });
        }

        await _context.SaveChangesAsync(ct);

        var updatedOrder = await _context.PurchaseOrders
            .Include(o => o.Supplier)
            .Include(o => o.Warehouse)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstAsync(o => o.Id == order.Id, ct);

        return MapToDetail(updatedOrder);
    }

    private static PurchaseOrderDetailDto MapToDetail(PurchaseOrder order)
    {
        var subtotal = order.Items.Sum(i => i.Quantity * i.UnitCost);
        var returnedValue = order.Items.Sum(i => i.ReturnedQuantity * i.UnitCost);

        return new PurchaseOrderDetailDto
        {
            Id = order.Id,
            ReferenceNumber = order.ReferenceNumber,
            BillNumber = order.BillNumber,
            BillDate = order.BillDate,
            DueDate = order.DueDate,
            PaymentStatus = order.PaymentStatus,
            PaymentMethod = order.PaymentMethod,
            OrderDate = order.OrderDate,
            Status = order.Status,
            HasReturns = order.HasReturns,
            SupplierId = order.SupplierId,
            SupplierName = order.Supplier?.Name ?? "",
            WarehouseId = order.WarehouseId,
            WarehouseName = order.Warehouse?.Name,
            Subtotal = subtotal,
            TotalAmount = order.TotalAmount,
            NetTotal = order.TotalAmount,
            ReturnedValue = returnedValue,
            ReturnStatus = order.HasReturns ? "returned" : null,
            Notes = order.Notes,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(i => new PurchaseOrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product?.Name,
                ProductSku = i.Product?.SKU,
                Quantity = i.Quantity,
                ReturnedQuantity = i.ReturnedQuantity,
                UnitCost = i.UnitCost,
                Total = i.Quantity * i.UnitCost,
            }).ToList(),
        };
    }
}

public class UpdatePurchaseOrderCommandValidator : AbstractValidator<UpdatePurchaseOrderCommand>
{
    public UpdatePurchaseOrderCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.SupplierId).NotEmpty();
        RuleFor(v => v.OrderDate).NotEmpty();
        RuleFor(v => v.Items).NotEmpty();
        RuleForEach(v => v.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitCost).GreaterThanOrEqualTo(0);
        });
    }
}
