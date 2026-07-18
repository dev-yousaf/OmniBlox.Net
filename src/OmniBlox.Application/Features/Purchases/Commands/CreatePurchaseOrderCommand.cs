using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Purchases.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Purchases.Commands;

public record CreatePurchaseOrderCommand : IRequest<PurchaseOrderDetailDto>
{
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

public class CreatePurchaseOrderCommandHandler : IRequestHandler<CreatePurchaseOrderCommand, PurchaseOrderDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public CreatePurchaseOrderCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PurchaseOrderDetailDto> Handle(CreatePurchaseOrderCommand request, CancellationToken ct)
    {
        var supplier = await _context.Suppliers.AsTracking().FirstOrDefaultAsync(s => s.Id == request.SupplierId, ct);
        if (supplier is null) throw new NotFoundException(nameof(Supplier), request.SupplierId);

        if (request.WarehouseId.HasValue)
        {
            var warehouse = await _context.Warehouses.AsTracking().FirstOrDefaultAsync(w => w.Id == request.WarehouseId.Value, ct);
            if (warehouse is null) throw new NotFoundException(nameof(Warehouse), request.WarehouseId.Value);
        }

        var referenceNumber = request.ReferenceNumber
            ?? $"PO-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{Random.Shared.Next(1000, 9999)}";

        var totalAmount = request.Items.Sum(i => i.Quantity * i.UnitCost);

        var order = new PurchaseOrder
        {
            SupplierId = request.SupplierId,
            UserId = _currentUser.UserId,
            OrderDate = request.OrderDate,
            ReferenceNumber = referenceNumber,
            BillNumber = request.BillNumber,
            BillDate = request.BillDate,
            DueDate = request.DueDate,
            PaymentStatus = request.PaymentStatus ?? "PENDING",
            PaymentMethod = request.PaymentMethod,
            Status = request.Status ?? "PENDING",
            Notes = request.Notes,
            WarehouseId = request.WarehouseId,
            TotalAmount = totalAmount,
        };

        _context.PurchaseOrders.Add(order);
        await _context.SaveChangesAsync(ct);

        var items = new List<PurchaseOrderItem>();
        foreach (var item in request.Items)
        {
            var product = await _context.Products.AsTracking().FirstOrDefaultAsync(p => p.Id == item.ProductId, ct);
            if (product is null) throw new NotFoundException(nameof(Product), item.ProductId);

            var poi = new PurchaseOrderItem
            {
                PurchaseOrderId = order.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitCost = item.UnitCost,
            };
            _context.PurchaseOrderItems.Add(poi);
            items.Add(poi);
        }

        await _context.SaveChangesAsync(ct);

        var orderWithItems = await _context.PurchaseOrders
            .Include(o => o.Supplier)
            .Include(o => o.Warehouse)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .AsTracking().FirstAsync(o => o.Id == order.Id, ct);

        return MapToDetail(orderWithItems);
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

public class CreatePurchaseOrderCommandValidator : AbstractValidator<CreatePurchaseOrderCommand>
{
    public CreatePurchaseOrderCommandValidator()
    {
        RuleFor(v => v.SupplierId).NotEmpty();
        RuleFor(v => v.OrderDate).NotEmpty();
        RuleFor(v => v.Items).NotEmpty();
        RuleForEach(v => v.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitCost).GreaterThanOrEqualTo(0);
        });
        RuleFor(v => v.ReferenceNumber).MaximumLength(100).When(v => v.ReferenceNumber is not null);
        RuleFor(v => v.BillNumber).MaximumLength(100).When(v => v.BillNumber is not null);
    }
}
