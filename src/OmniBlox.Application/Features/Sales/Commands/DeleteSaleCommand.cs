using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;
using Inv = OmniBlox.Domain.Entities.Inventory;

namespace OmniBlox.Application.Features.Sales.Commands;

public record DeleteSaleCommand : IRequest
{
    public Guid Id { get; init; }
}

public class DeleteSaleCommandHandler : IRequestHandler<DeleteSaleCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public DeleteSaleCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteSaleCommand request, CancellationToken ct)
    {
        var sale = await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == _currentUser.CompanyId, ct);

        if (sale is null)
            throw new NotFoundException(nameof(Sale), request.Id);

        if (sale.Status == "COMPLETED")
        {
            foreach (var item in sale.Items)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(x => x.ProductId == item.ProductId && x.WarehouseId == sale.WarehouseId, ct);

                if (inventory is null)
                {
                    inventory = new Inv
                    {
                        ProductId = item.ProductId,
                        WarehouseId = sale.WarehouseId!.Value,
                        Quantity = 0,
                    };
                    _context.Inventories.Add(inventory);
                }

                inventory.Quantity += item.Quantity;

                _context.StockLedgerEntries.Add(new StockLedgerEntry
                {
                    Quantity = item.Quantity,
                    Balance = inventory.Quantity,
                    Type = "SALE_DELETE",
                    Reference = sale.InvoiceNumber,
                    ProductId = item.ProductId,
                    WarehouseId = sale.WarehouseId,
                });

                var product = await _context.Products.FirstAsync(x => x.Id == item.ProductId, ct);
                product.Stock += item.Quantity;
            }
        }

        _context.Sales.Remove(sale);
        await _context.SaveChangesAsync(ct);
    }
}

public class DeleteSaleCommandValidator : AbstractValidator<DeleteSaleCommand>
{
    public DeleteSaleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
