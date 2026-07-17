using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Sales.Commands;

public record DeleteSaleCommand : IRequest
{
    public Guid Id { get; init; }
}

public class DeleteSaleCommandHandler : IRequestHandler<DeleteSaleCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IStockService _stockService;

    public DeleteSaleCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IStockService stockService)
    {
        _context = context;
        _currentUser = currentUser;
        _stockService = stockService;
    }

    public async Task Handle(DeleteSaleCommand request, CancellationToken ct)
    {
        var sale = await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == _currentUser.CompanyId, ct);

        if (sale is null)
            throw new NotFoundException(nameof(Sale), request.Id);

        if (sale.Status == "COMPLETED" && sale.WarehouseId.HasValue)
        {
            foreach (var item in sale.Items)
            {
                await _stockService.RecordMovementAsync(new RecordMovementArgs
                {
                    ProductId = item.ProductId,
                    WarehouseId = sale.WarehouseId.Value,
                    MovementType = MovementType.sale_return,
                    Quantity = item.Quantity,
                    ReferenceType = "sale",
                    ReferenceId = sale.Id,
                    UserId = _currentUser.UserId,
                }, ct);
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
