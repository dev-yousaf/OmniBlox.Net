using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Products.DTOs;

namespace OmniBlox.Application.Features.Products.Queries;

public record GetStockLedgerQuery : IRequest<List<StockLedgerEntryDto>>
{
    public Guid ProductId { get; init; }
}

public class GetStockLedgerQueryHandler : IRequestHandler<GetStockLedgerQuery, List<StockLedgerEntryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetStockLedgerQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<StockLedgerEntryDto>> Handle(GetStockLedgerQuery request, CancellationToken ct)
    {
        var stockMovements = await _context.StockMovements
            .Include(m => m.Warehouse)
            .Where(m => m.ProductId == request.ProductId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);

        var movements = stockMovements.Select(m => new StockLedgerEntryDto
        {
            Id = m.Id,
            Quantity = m.MovementType switch
            {
                Domain.Enums.MovementType.sale or Domain.Enums.MovementType.purchase_return
                    or Domain.Enums.MovementType.adjustment_out or Domain.Enums.MovementType.transfer_out
                    => -m.Quantity,
                _ => m.Quantity,
            },
            Balance = m.BalanceAfter,
            Type = m.MovementType.ToString(),
            Reference = m.ReferenceType,
            Note = $"{m.Warehouse?.Name}",
            CreatedAt = m.CreatedAt,
            ProductId = m.ProductId,
        }).ToList();

        return movements.OrderByDescending(m => m.CreatedAt).ToList();
    }
}
