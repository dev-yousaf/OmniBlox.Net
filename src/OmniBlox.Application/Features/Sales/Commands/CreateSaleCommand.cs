using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Sales.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;
using Inv = OmniBlox.Domain.Entities.Inventory;

namespace OmniBlox.Application.Features.Sales.Commands;

public record CreateSaleCommand : IRequest<SaleDetailDto>
{
    public string? InvoiceNumber { get; init; }
    public Guid CustomerId { get; init; }
    public Guid WarehouseId { get; init; }
    public DateTime SaleDate { get; init; }
    public DateTime DueDate { get; init; }
    public string? Status { get; init; }
    public string? PaymentStatus { get; init; }
    public string? PaymentMethod { get; init; }
    public decimal TaxRate { get; init; }
    public decimal Discount { get; init; }
    public string? Notes { get; init; }
    public string? ShippingAddress { get; init; }
    public List<CreateSaleItem> Items { get; init; } = new();
}

public class CreateSaleCommandHandler : IRequestHandler<CreateSaleCommand, SaleDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public CreateSaleCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SaleDetailDto> Handle(CreateSaleCommand request, CancellationToken ct)
    {
        var warehouse = await _context.Warehouses.FirstOrDefaultAsync(x => x.Id == request.WarehouseId, ct);
        if (warehouse is null)
            throw new NotFoundException(nameof(Warehouse), request.WarehouseId);

        var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Id == request.CustomerId, ct);
        if (customer is null)
            throw new NotFoundException(nameof(Customer), request.CustomerId);

        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync(ct);
        if (products.Count != productIds.Count)
        {
            var missing = productIds.Except(products.Select(p => p.Id)).ToList();
            throw new NotFoundException(nameof(Product), string.Join(",", missing));
        }

        if (request.Status == "COMPLETED")
        {
            foreach (var item in request.Items)
            {
                var product = products.First(p => p.Id == item.ProductId);
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(x => x.ProductId == item.ProductId && x.WarehouseId == request.WarehouseId, ct);
                var availableQty = inventory?.Quantity ?? 0;
                if (item.Quantity > availableQty)
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock for product '{product.Name}'. Available: {availableQty}, requested: {item.Quantity}.");
                }
            }
        }

        var invoiceNumber = request.InvoiceNumber ??
            $"INV-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{Random.Shared.Next(1000, 9999)}";

        var subtotal = request.Items.Sum(i => i.Quantity * i.UnitPrice);
        var tax = subtotal * request.TaxRate / 100m;
        var total = subtotal + tax - request.Discount;

        var sale = new Sale
        {
            InvoiceNumber = invoiceNumber,
            CustomerId = request.CustomerId,
            UserId = _currentUser.UserId,
            WarehouseId = request.WarehouseId,
            SaleDate = request.SaleDate,
            DueDate = request.DueDate,
            Status = request.Status ?? "DRAFT",
            PaymentStatus = request.PaymentStatus ?? "PENDING",
            PaymentMethod = request.PaymentMethod,
            TaxRate = request.TaxRate,
            Discount = request.Discount,
            Subtotal = subtotal,
            Tax = tax,
            TotalAmount = total,
            Notes = request.Notes,
            ShippingAddress = request.ShippingAddress,
        };

        sale.Items = request.Items.Select(i => new SaleItem
        {
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
        }).ToList();

        _context.Sales.Add(sale);

        if (sale.Status == "COMPLETED")
        {
            foreach (var item in sale.Items)
            {
                await DecrementInventory(item.ProductId, item.Quantity, request.WarehouseId, invoiceNumber, ct);
            }
        }

        await _context.SaveChangesAsync(ct);

        var result = await _context.Sales
            .Include(s => s.Customer)
            .Include(s => s.Warehouse)
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .FirstAsync(x => x.Id == sale.Id, ct);

        return SaleDetailDto.FromEntity(result);
    }

    private async Task DecrementInventory(Guid productId, int quantity, Guid warehouseId, string reference, CancellationToken ct)
    {
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.WarehouseId == warehouseId, ct);

        if (inventory is null)
        {
            inventory = new Inv
            {
                ProductId = productId,
                WarehouseId = warehouseId,
                Quantity = 0,
            };
            _context.Inventories.Add(inventory);
        }

        inventory.Quantity -= quantity;

        var balance = inventory.Quantity;

        _context.StockLedgerEntries.Add(new StockLedgerEntry
        {
            Quantity = -quantity,
            Balance = balance,
            Type = "SALE",
            Reference = reference,
            ProductId = productId,
            WarehouseId = warehouseId,
        });

        var product = await _context.Products.FirstAsync(x => x.Id == productId, ct);
        product.Stock -= quantity;
    }
}

public class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.SaleDate).NotEmpty();
        RuleFor(x => x.DueDate).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
        });
        RuleFor(x => x.Discount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TaxRate).GreaterThanOrEqualTo(0);
    }
}
