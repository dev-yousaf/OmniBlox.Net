using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Products.Commands;

public record AdjustStockCommand : IRequest<AdjustStockResponse>
{
    public List<AdjustStockItem> Items { get; init; } = [];
    public string? Notes { get; init; }
    public string Type { get; init; } = "ADDITION";
    public string? DocumentUrl { get; init; }
}

public record AdjustStockItem
{
    public Guid ProductId { get; init; }
    public Guid? WarehouseId { get; init; }
    public int PreviousQuantity { get; init; }
    public int NewQuantity { get; init; }
}

public record AdjustStockResponse
{
    public string Message { get; init; } = string.Empty;
    public int AdjustedCount { get; init; }
}

public class AdjustStockCommandHandler : IRequestHandler<AdjustStockCommand, AdjustStockResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IStockService _stockService;

    public AdjustStockCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IStockService stockService)
    {
        _context = context;
        _currentUser = currentUser;
        _stockService = stockService;
    }

    public async Task<AdjustStockResponse> Handle(AdjustStockCommand request, CancellationToken ct)
    {
        var adjusted = 0;

        foreach (var item in request.Items)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId, ct);
            if (product is null) continue;

            if (!item.WarehouseId.HasValue) continue;

            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.WarehouseId == item.WarehouseId.Value, ct);

            var currentQty = inventory?.Quantity ?? 0;
            var difference = item.NewQuantity - currentQty;

            if (difference == 0) continue;

            var movementType = difference > 0 ? MovementType.adjustment_in : MovementType.adjustment_out;

            await _stockService.RecordMovementAsync(new RecordMovementArgs
            {
                ProductId = item.ProductId,
                WarehouseId = item.WarehouseId.Value,
                MovementType = movementType,
                Quantity = Math.Abs(difference),
                ReferenceType = "adjustment",
                ReferenceId = product.Id,
                UserId = _currentUser.UserId,
            }, ct);

            adjusted++;
        }

        await _context.SaveChangesAsync(ct);
        return new AdjustStockResponse
        {
            Message = $"{adjusted} product(s) adjusted.",
            AdjustedCount = adjusted,
        };
    }
}