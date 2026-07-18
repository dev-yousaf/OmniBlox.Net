using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Purchases.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Purchases.Commands;

public record ReceivePurchaseOrderCommand : IRequest<PurchaseOrderDetailDto>
{
    public Guid Id { get; init; }
    public Guid WarehouseId { get; init; }
}

public class ReceivePurchaseOrderCommandHandler : IRequestHandler<ReceivePurchaseOrderCommand, PurchaseOrderDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IStockService _stockService;

    public ReceivePurchaseOrderCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IStockService stockService)
    {
        _context = context;
        _currentUser = currentUser;
        _stockService = stockService;
    }

    public async Task<PurchaseOrderDetailDto> Handle(ReceivePurchaseOrderCommand request, CancellationToken ct)
    {
        var order = await _context.PurchaseOrders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Supplier)
            .Include(o => o.Warehouse)
            .AsTracking().FirstOrDefaultAsync(o => o.Id == request.Id, ct);
        if (order is null) throw new NotFoundException(nameof(PurchaseOrder), request.Id);

        if (order.Status == "COMPLETED")
            throw new InvalidOperationException("Purchase order is already completed.");

        var warehouse = await _context.Warehouses.AsTracking().FirstOrDefaultAsync(w => w.Id == request.WarehouseId, ct);
        if (warehouse is null) throw new NotFoundException(nameof(Warehouse), request.WarehouseId);

        order.WarehouseId = request.WarehouseId;
        order.Status = "COMPLETED";
        order.UpdatedAt = DateTime.UtcNow;

        foreach (var item in order.Items)
        {
            await _stockService.RecordMovementAsync(new RecordMovementArgs
            {
                ProductId = item.ProductId,
                WarehouseId = request.WarehouseId,
                MovementType = MovementType.purchase,
                Quantity = item.Quantity,
                ReferenceType = "purchase",
                ReferenceId = order.Id,
                UserId = _currentUser.UserId,
            }, ct);
        }

        await _context.SaveChangesAsync(ct);

        var updatedOrder = await _context.PurchaseOrders
            .Include(o => o.Supplier)
            .Include(o => o.Warehouse)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .AsTracking().FirstAsync(o => o.Id == order.Id, ct);

        var subtotal = updatedOrder.Items.Sum(i => i.Quantity * i.UnitCost);
        var returnedValue = updatedOrder.Items.Sum(i => i.ReturnedQuantity * i.UnitCost);

        return new PurchaseOrderDetailDto
        {
            Id = updatedOrder.Id,
            ReferenceNumber = updatedOrder.ReferenceNumber,
            BillNumber = updatedOrder.BillNumber,
            BillDate = updatedOrder.BillDate,
            DueDate = updatedOrder.DueDate,
            PaymentStatus = updatedOrder.PaymentStatus,
            PaymentMethod = updatedOrder.PaymentMethod,
            OrderDate = updatedOrder.OrderDate,
            Status = updatedOrder.Status,
            HasReturns = updatedOrder.HasReturns,
            SupplierId = updatedOrder.SupplierId,
            SupplierName = updatedOrder.Supplier?.Name ?? "",
            WarehouseId = updatedOrder.WarehouseId,
            WarehouseName = updatedOrder.Warehouse?.Name,
            Subtotal = subtotal,
            TotalAmount = updatedOrder.TotalAmount,
            NetTotal = updatedOrder.TotalAmount,
            ReturnedValue = returnedValue,
            ReturnStatus = updatedOrder.HasReturns ? "returned" : null,
            Notes = updatedOrder.Notes,
            CreatedAt = updatedOrder.CreatedAt,
            Items = updatedOrder.Items.Select(i => new PurchaseOrderItemDto
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

public class ReceivePurchaseOrderCommandValidator : AbstractValidator<ReceivePurchaseOrderCommand>
{
    public ReceivePurchaseOrderCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.WarehouseId).NotEmpty();
    }
}
