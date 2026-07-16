using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;
using Inv = OmniBlox.Domain.Entities.Inventory;

namespace OmniBlox.Application.Features.SalesReturns.Commands;

public record DeleteSalesReturnCommand : IRequest
{
    public Guid Id { get; init; }
}

public class DeleteSalesReturnCommandHandler : IRequestHandler<DeleteSalesReturnCommand>
{
    private readonly IApplicationDbContext _context;
    public DeleteSalesReturnCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(DeleteSalesReturnCommand request, CancellationToken ct)
    {
        var salesReturn = await _context.SalesReturns
            .Include(r => r.Items)
            .Include(r => r.Sale)
                .ThenInclude(s => s!.Items)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (salesReturn is null)
            throw new NotFoundException(nameof(SalesReturn), request.Id);

        if (salesReturn.Status == "COMPLETED")
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

                inventory.Quantity -= item.Quantity;

                _context.StockLedgerEntries.Add(new StockLedgerEntry
                {
                    Quantity = -item.Quantity,
                    Balance = inventory.Quantity,
                    Type = "RETURN_DELETE",
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

        _context.SalesReturns.Remove(salesReturn);
        await _context.SaveChangesAsync(ct);
    }
}

public class DeleteSalesReturnCommandValidator : AbstractValidator<DeleteSalesReturnCommand>
{
    public DeleteSalesReturnCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
