using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Sales.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;
using Inv = OmniBlox.Domain.Entities.Inventory;

namespace OmniBlox.Application.Features.Sales.Commands;

public record UpdateSaleCommand : IRequest<SaleDetailDto>
{
    public Guid Id { get; init; }
    public string? InvoiceNumber { get; init; }
    public Guid CustomerId { get; init; }
    public Guid? WarehouseId { get; init; }
    public DateTime SaleDate { get; init; }
    public DateTime DueDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? PaymentStatus { get; init; }
    public string? PaymentMethod { get; init; }
    public decimal TaxRate { get; init; }
    public decimal Discount { get; init; }
    public string? Notes { get; init; }
    public string? ShippingAddress { get; init; }
    public List<CreateSaleItem> Items { get; init; } = new();
}

public class UpdateSaleCommandHandler : IRequestHandler<UpdateSaleCommand, SaleDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public UpdateSaleCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SaleDetailDto> Handle(UpdateSaleCommand request, CancellationToken ct)
    {
        var sale = await _context.Sales
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .Include(s => s.Customer)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == _currentUser.CompanyId, ct);

        if (sale is null)
            throw new NotFoundException(nameof(Sale), request.Id);

        if (request.WarehouseId.HasValue)
        {
            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(x => x.Id == request.WarehouseId.Value && x.CompanyId == _currentUser.CompanyId, ct);
            if (warehouse is null)
                throw new NotFoundException(nameof(Warehouse), request.WarehouseId.Value);
        }

        var customer = await _context.Customers
            .FirstOrDefaultAsync(x => x.Id == request.CustomerId && x.CompanyId == _currentUser.CompanyId, ct);
        if (customer is null)
            throw new NotFoundException(nameof(Customer), request.CustomerId);

        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync(ct);
        if (products.Count != productIds.Count)
        {
            var missing = productIds.Except(products.Select(p => p.Id)).ToList();
            throw new NotFoundException(nameof(Product), string.Join(",", missing));
        }

        var oldStatus = sale.Status;
        var oldItems = sale.Items.ToList();
        var oldInvoiceNumber = sale.InvoiceNumber;
        var oldWarehouseId = sale.WarehouseId;
        var invoiceNumber = request.InvoiceNumber ?? oldInvoiceNumber;

        var subtotal = request.Items.Sum(i => i.Quantity * i.UnitPrice);
        var tax = subtotal * request.TaxRate / 100m;
        var total = subtotal + tax - request.Discount;

        sale.InvoiceNumber = invoiceNumber;
        sale.CustomerId = request.CustomerId;
        sale.WarehouseId = request.WarehouseId;
        sale.SaleDate = request.SaleDate;
        sale.DueDate = request.DueDate;
        sale.PaymentMethod = request.PaymentMethod;
        sale.TaxRate = request.TaxRate;
        sale.Discount = request.Discount;
        sale.Subtotal = subtotal;
        sale.Tax = tax;
        sale.TotalAmount = total;
        sale.Notes = request.Notes;
        sale.ShippingAddress = request.ShippingAddress;

        // Restore stock if the sale was previously completed
        if (oldStatus == "COMPLETED")
        {
            if (oldWarehouseId.HasValue)
            {
                foreach (var item in oldItems)
                {
                    await RestoreInventory(item.ProductId, item.Quantity, oldWarehouseId.Value, oldInvoiceNumber, ct);
                }
            }
        }

        var oldSaleItems = await _context.SaleItems
            .Where(si => si.SaleId == sale.Id)
            .ToListAsync(ct);
        _context.SaleItems.RemoveRange(oldSaleItems);
        sale.Items.Clear();

        foreach (var item in request.Items)
        {
            var newSaleItem = new SaleItem
            {
                SaleId = sale.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
            };
            _context.SaleItems.Add(newSaleItem);
            sale.Items.Add(newSaleItem);
        }

        // Decrement stock for completed status (works for both new and previously completed sales)
        if (request.Status == "COMPLETED")
        {
            var decrementWh = request.WarehouseId ?? sale.WarehouseId;
            if (decrementWh.HasValue)
            {
                foreach (var item in sale.Items)
                {
                    var inventory = await _context.Inventories
                        .FirstOrDefaultAsync(x => x.ProductId == item.ProductId && x.WarehouseId == decrementWh.Value, ct);
                    var availableQty = inventory?.Quantity ?? 0;
                    if (item.Quantity > availableQty)
                    {
                        var product = await _context.Products.FindAsync(new object[] { item.ProductId }, ct);
                        throw new InvalidOperationException(
                            $"Insufficient stock for product '{product?.Name ?? item.ProductId.ToString()}'. Available: {availableQty}, requested: {item.Quantity}.");
                    }
                }

                foreach (var item in sale.Items)
                {
                    await DecrementInventory(item.ProductId, item.Quantity, decrementWh.Value, invoiceNumber, ct);
                }
            }
        }

        sale.Status = request.Status;
        if (request.PaymentStatus is not null)
            sale.PaymentStatus = request.PaymentStatus;

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

        _context.StockLedgerEntries.Add(new StockLedgerEntry
        {
            Quantity = -quantity,
            Balance = inventory.Quantity,
            Type = "SALE",
            Reference = reference,
            ProductId = productId,
            WarehouseId = warehouseId,
        });

        var product = await _context.Products.FirstAsync(x => x.Id == productId, ct);
        product.Stock -= quantity;
    }

    private async Task RestoreInventory(Guid productId, int quantity, Guid warehouseId, string reference, CancellationToken ct)
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

        inventory.Quantity += quantity;

        _context.StockLedgerEntries.Add(new StockLedgerEntry
        {
            Quantity = quantity,
            Balance = inventory.Quantity,
            Type = "SALE_REVERSAL",
            Reference = reference,
            ProductId = productId,
            WarehouseId = warehouseId,
        });

        var product = await _context.Products.FirstAsync(x => x.Id == productId, ct);
        product.Stock += quantity;
    }
}

public class UpdateSaleCommandValidator : AbstractValidator<UpdateSaleCommand>
{
    public UpdateSaleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
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
        RuleFor(x => x.Status).NotEmpty();
    }
}
