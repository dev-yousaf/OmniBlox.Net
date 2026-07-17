using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.SalesReturns.Commands;

public record DeleteSalesReturnCommand : IRequest
{
    public Guid Id { get; init; }
}

public class DeleteSalesReturnCommandHandler : IRequestHandler<DeleteSalesReturnCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IStockService _stockService;

    public DeleteSalesReturnCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IStockService stockService)
    {
        _context = context;
        _currentUser = currentUser;
        _stockService = stockService;
    }

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

                var availableQty = inventory?.Quantity ?? 0;
                if (item.Quantity > availableQty)
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock for product '{item.Product?.Name}'. Available: {availableQty}, requested: {item.Quantity}.");
                }

                await _stockService.RecordMovementAsync(new RecordMovementArgs
                {
                    ProductId = item.ProductId,
                    WarehouseId = salesReturn.WarehouseId,
                    MovementType = MovementType.sale,
                    Quantity = item.Quantity,
                    ReferenceType = "sale_return",
                    ReferenceId = salesReturn.Id,
                    UserId = _currentUser.UserId,
                }, ct);

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
