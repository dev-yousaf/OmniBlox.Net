using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Quotations.Commands;

public record ConvertQuotationToSaleCommand : IRequest<QuotationSaleResult>
{
    public Guid QuotationId { get; init; }
    public string Status { get; init; } = "COMPLETED";
    public string PaymentStatus { get; init; } = "PENDING";
    public string? PaymentMethod { get; init; }
    public DateTime SaleDate { get; init; }
    public DateTime DueDate { get; init; }
    public string? ShippingAddress { get; init; }
    public string? Notes { get; init; }
    public Guid? WarehouseId { get; init; }
    public Guid? BillerId { get; init; }
}

public record QuotationSaleResult
{
    public Guid SaleId { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public string Message { get; init; } = string.Empty;
}

public class ConvertQuotationToSaleCommandHandler : IRequestHandler<ConvertQuotationToSaleCommand, QuotationSaleResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IStockService _stockService;

    public ConvertQuotationToSaleCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IStockService stockService)
    {
        _context = context;
        _currentUser = currentUser;
        _stockService = stockService;
    }

    public async Task<QuotationSaleResult> Handle(ConvertQuotationToSaleCommand request, CancellationToken ct)
    {
        var quotation = await _context.Quotations
            .Include(q => q.Items).ThenInclude(i => i.Product)
            .Include(q => q.Customer)
            .FirstOrDefaultAsync(q => q.Id == request.QuotationId, ct);

        if (quotation is null)
            throw new NotFoundException(nameof(Quotation), request.QuotationId);

        var invoiceNumber = "INV-" +
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + "-" +
            Random.Shared.Next(1000, 9999);

        var saleItems = quotation.Items.Select(item => new SaleItem
        {
            ProductId = item.ProductId,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
        }).ToList();

        var subtotal = saleItems.Sum(i => i.Quantity * i.UnitPrice);

        var sale = new Sale
        {
            InvoiceNumber = invoiceNumber,
            CustomerId = quotation.CustomerId,
            SaleDate = request.SaleDate,
            DueDate = request.DueDate,
            Status = request.Status,
            PaymentStatus = request.PaymentStatus,
            PaymentMethod = request.PaymentMethod,
            Subtotal = subtotal,
            TotalAmount = subtotal,
            ShippingAddress = request.ShippingAddress,
            Notes = request.Notes,
            SourceQuotationId = quotation.Id,
            WarehouseId = request.WarehouseId,
            BillerId = request.BillerId,
            Items = saleItems,
        };

        _context.Sales.Add(sale);

        if (request.Status == "COMPLETED" && request.WarehouseId.HasValue)
        {
            foreach (var item in quotation.Items)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i =>
                        i.ProductId == item.ProductId &&
                        i.WarehouseId == request.WarehouseId.Value, ct);

                var availableQty = inventory?.Quantity ?? 0;
                if (item.Quantity > availableQty)
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock for product '{item.Product?.Name}'. Available: {availableQty}, requested: {item.Quantity}.");
                }

                await _stockService.RecordMovementAsync(new RecordMovementArgs
                {
                    ProductId = item.ProductId,
                    WarehouseId = request.WarehouseId.Value,
                    MovementType = MovementType.sale,
                    Quantity = item.Quantity,
                    ReferenceType = "sale",
                    ReferenceId = sale.Id,
                    UserId = _currentUser.UserId,
                }, ct);
            }
        }

        await _context.SaveChangesAsync(ct);

        return new QuotationSaleResult
        {
            SaleId = sale.Id,
            InvoiceNumber = invoiceNumber,
            TotalAmount = subtotal,
            Message = $"Successfully converted quotation {quotation.ReferenceNumber} to sale {invoiceNumber}.",
        };
    }
}

public class ConvertQuotationToSaleCommandValidator : AbstractValidator<ConvertQuotationToSaleCommand>
{
    public ConvertQuotationToSaleCommandValidator()
    {
        RuleFor(v => v.QuotationId)
            .NotEmpty();

        RuleFor(v => v.SaleDate)
            .NotEmpty();

        RuleFor(v => v.PaymentStatus)
            .NotEmpty();
    }
}
