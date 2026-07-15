using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;

namespace OmniBlox.Application.Features.Products.Queries;

public record GetStockLedgerQuery : IRequest<List<StockLedgerEntryDto>>
{
    public Guid ProductId { get; init; }
}

public record StockLedgerEntryDto
{
    public Guid Id { get; init; }
    public int Quantity { get; init; }
    public int Balance { get; init; }
    public string Type { get; init; } = string.Empty;
    public string? Reference { get; init; }
    public string? Note { get; init; }
    public DateTime CreatedAt { get; init; }
    public Guid ProductId { get; init; }
}

public class GetStockLedgerQueryHandler : IRequestHandler<GetStockLedgerQuery, List<StockLedgerEntryDto>>
{
    private readonly IApplicationDbContext _context;
    public GetStockLedgerQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<StockLedgerEntryDto>> Handle(GetStockLedgerQuery request, CancellationToken ct)
    {
        var entries = await _context.StockLedgerEntries
            .Where(e => e.ProductId == request.ProductId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(ct);

        return entries.Select(e => new StockLedgerEntryDto
        {
            Id = e.Id,
            Quantity = e.Quantity,
            Balance = e.Balance,
            Type = e.Type,
            Reference = e.Reference,
            Note = e.Note,
            CreatedAt = e.CreatedAt,
            ProductId = e.ProductId,
        }).ToList();
    }
}
