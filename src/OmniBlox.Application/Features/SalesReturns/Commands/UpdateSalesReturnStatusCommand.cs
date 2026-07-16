using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.SalesReturns.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;
using Inv = OmniBlox.Domain.Entities.Inventory;

namespace OmniBlox.Application.Features.SalesReturns.Commands;

public record UpdateSalesReturnStatusCommand : IRequest<SalesReturnDetailDto>
{
    public Guid Id { get; init; }
    public string Status { get; init; } = string.Empty;
}

public class UpdateSalesReturnStatusCommandHandler : IRequestHandler<UpdateSalesReturnStatusCommand, SalesReturnDetailDto>
{
    private readonly IApplicationDbContext _context;
    public UpdateSalesReturnStatusCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<SalesReturnDetailDto> Handle(UpdateSalesReturnStatusCommand request, CancellationToken ct)
    {
        var salesReturn = await _context.SalesReturns
            .Include(r => r.Items)
                .ThenInclude(i => i.Product)
            .Include(r => r.Sale)
                .ThenInclude(s => s!.Items)
            .Include(r => r.Warehouse)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (salesReturn is null)
            throw new NotFoundException(nameof(SalesReturn), request.Id);

        var oldStatus = salesReturn.Status;

        if (oldStatus == request.Status)
            throw new ConflictException($"Sales return is already {request.Status}.");

        if (oldStatus == "COMPLETED" && request.Status == "CANCELLED")
        {
            foreach (var item in salesReturn.Items)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(x => x.ProductId == item.ProductId && x.WarehouseId == salesReturn.WarehouseId, ct);

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
                        WarehouseId = salesReturn.WarehouseId,
                        Quantity = 0,
                    };
                    _context.Inventories.Add(inventory);
                }

                inventory.Quantity -= item.Quantity;

                _context.StockLedgerEntries.Add(new StockLedgerEntry
                {
                    Quantity = -item.Quantity,
                    Balance = inventory.Quantity,
                    Type = "RETURN_REVERSAL",
                    Reference = salesReturn.ReferenceNumber,
                    ProductId = item.ProductId,
                    WarehouseId = salesReturn.WarehouseId,
                });

                var product = await _context.Products.FirstAsync(x => x.Id == item.ProductId, ct);
                product.Stock -= item.Quantity;

                if (item.SaleItemId.HasValue && salesReturn.Sale is not null)
                {
                    var saleItem = salesReturn.Sale.Items.FirstOrDefault(si => si.Id == item.SaleItemId.Value);
                    if (saleItem is not null)
                    {
                        saleItem.ReturnedQuantity -= item.Quantity;
                    }
                }
            }

            if (salesReturn.Sale is not null)
                salesReturn.Sale.HasReturns = salesReturn.Sale.Items.Any(si => si.ReturnedQuantity > 0);
        }
        else if ((oldStatus == "PENDING" || oldStatus == "PROCESSING") && request.Status == "COMPLETED")
        {
            foreach (var item in salesReturn.Items)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(x => x.ProductId == item.ProductId && x.WarehouseId == salesReturn.WarehouseId, ct);

                if (inventory is null)
                {
                    inventory = new Inv
                    {
                        ProductId = item.ProductId,
                        WarehouseId = salesReturn.WarehouseId,
                        Quantity = 0,
                    };
                    _context.Inventories.Add(inventory);
                }

                inventory.Quantity += item.Quantity;

                _context.StockLedgerEntries.Add(new StockLedgerEntry
                {
                    Quantity = item.Quantity,
                    Balance = inventory.Quantity,
                    Type = "RETURN",
                    Reference = salesReturn.ReferenceNumber,
                    ProductId = item.ProductId,
                    WarehouseId = salesReturn.WarehouseId,
                });

                var product = await _context.Products.FirstAsync(x => x.Id == item.ProductId, ct);
                product.Stock += item.Quantity;

                if (item.SaleItemId.HasValue && salesReturn.Sale is not null)
                {
                    var saleItem = salesReturn.Sale.Items.FirstOrDefault(si => si.Id == item.SaleItemId.Value);
                    if (saleItem is not null)
                    {
                        saleItem.ReturnedQuantity += item.Quantity;
                    }
                }
            }

            if (salesReturn.Sale is not null)
                salesReturn.Sale.HasReturns = true;
        }

        salesReturn.Status = request.Status;
        await _context.SaveChangesAsync(ct);

        var result = await _context.SalesReturns
            .Include(r => r.Warehouse)
            .Include(r => r.Sale)
            .Include(r => r.Items)
                .ThenInclude(i => i.Product)
            .FirstAsync(x => x.Id == salesReturn.Id, ct);

        return SalesReturnDetailDto.FromEntity(result);
    }
}

public class UpdateSalesReturnStatusCommandValidator : AbstractValidator<UpdateSalesReturnStatusCommand>
{
    public UpdateSalesReturnStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Status).NotEmpty()
            .Must(s => s is "COMPLETED" or "CANCELLED")
            .WithMessage("Status must be COMPLETED or CANCELLED.");
    }
}
