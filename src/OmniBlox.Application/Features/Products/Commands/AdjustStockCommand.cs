using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
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
    public AdjustStockCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<AdjustStockResponse> Handle(AdjustStockCommand request, CancellationToken ct)
    {
        var adjusted = 0;

        foreach (var item in request.Items)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId, ct);
            if (product is null) continue;

            var difference = item.NewQuantity - item.PreviousQuantity;
            product.Stock = item.NewQuantity;
            product.UpdatedAt = DateTime.UtcNow;

            _context.StockLedgerEntries.Add(new StockLedgerEntry
            {
                ProductId = product.Id,
                Quantity = difference,
                Balance = product.Stock,
                Type = request.Type,
                Reference = "Stock adjustment",
                Note = request.Notes,
            });
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
