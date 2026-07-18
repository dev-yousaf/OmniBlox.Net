using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Purchases.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Purchases.Commands;

public record MarkPurchasePaidCommand : IRequest<PurchaseOrderDetailDto>
{
    public Guid Id { get; init; }
}

public class MarkPurchasePaidCommandHandler : IRequestHandler<MarkPurchasePaidCommand, PurchaseOrderDetailDto>
{
    private readonly IApplicationDbContext _context;
    public MarkPurchasePaidCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<PurchaseOrderDetailDto> Handle(MarkPurchasePaidCommand request, CancellationToken ct)
    {
        var order = await _context.PurchaseOrders
            .Include(o => o.Supplier)
            .Include(o => o.Warehouse)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .AsTracking().FirstOrDefaultAsync(o => o.Id == request.Id, ct);
        if (order is null) throw new NotFoundException(nameof(PurchaseOrder), request.Id);

        order.PaymentStatus = "PAID";
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

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

public class MarkPurchasePaidCommandValidator : AbstractValidator<MarkPurchasePaidCommand>
{
    public MarkPurchasePaidCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
    }
}
