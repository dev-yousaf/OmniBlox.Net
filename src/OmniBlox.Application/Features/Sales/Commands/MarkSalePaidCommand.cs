using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Sales.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Sales.Commands;

public record MarkSalePaidCommand : IRequest<SaleDetailDto>
{
    public Guid Id { get; init; }
}

public class MarkSalePaidCommandHandler : IRequestHandler<MarkSalePaidCommand, SaleDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IStockService _stockService;

    public MarkSalePaidCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IStockService stockService)
    {
        _context = context;
        _currentUser = currentUser;
        _stockService = stockService;
    }

    public async Task<SaleDetailDto> Handle(MarkSalePaidCommand request, CancellationToken ct)
    {
        var sale = await _context.Sales
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .Include(s => s.Customer)
            .Include(s => s.Warehouse)
            .AsTracking().FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == _currentUser.CompanyId, ct);

        if (sale is null)
            throw new NotFoundException(nameof(Sale), request.Id);

        var wasPending = sale.Status != "COMPLETED";

        sale.PaymentStatus = "PAID";
        sale.Status = "COMPLETED";

        if (wasPending && sale.WarehouseId.HasValue)
        {
            foreach (var item in sale.Items)
            {
                var inventory = await _context.Inventories
                    .AsTracking().FirstOrDefaultAsync(x => x.ProductId == item.ProductId && x.WarehouseId == sale.WarehouseId, ct);

                var availableQty = inventory?.Quantity ?? 0;
                if (item.Quantity > availableQty)
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock for product '{item.Product?.Name}'. Available: {availableQty}, requested: {item.Quantity}.");
                }

                await _stockService.RecordMovementAsync(new RecordMovementArgs
                {
                    ProductId = item.ProductId,
                    WarehouseId = sale.WarehouseId.Value,
                    MovementType = MovementType.sale,
                    Quantity = item.Quantity,
                    ReferenceType = "sale",
                    ReferenceId = sale.Id,
                    UserId = _currentUser.UserId,
                }, ct);
            }
        }

        await _context.SaveChangesAsync(ct);

        var result = await _context.Sales
            .Include(s => s.Customer)
            .Include(s => s.Warehouse)
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .AsTracking().FirstAsync(x => x.Id == sale.Id, ct);

        return SaleDetailDto.FromEntity(result);
    }
}

public class MarkSalePaidCommandValidator : AbstractValidator<MarkSalePaidCommand>
{
    public MarkSalePaidCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
