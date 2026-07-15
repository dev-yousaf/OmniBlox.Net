using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Inventory.DTOs;

namespace OmniBlox.Application.Features.Inventory.Queries;

public record GetStockTransferByIdQuery : IRequest<StockTransferDto>
{
    public Guid Id { get; init; }
}

public class GetStockTransferByIdQueryHandler : IRequestHandler<GetStockTransferByIdQuery, StockTransferDto>
{
    private readonly IApplicationDbContext _context;
    public GetStockTransferByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<StockTransferDto> Handle(GetStockTransferByIdQuery request, CancellationToken ct)
    {
        var entry = await _context.StockLedgerEntries
            .Include(l => l.Product)
            .Include(l => l.Warehouse)
            .FirstOrDefaultAsync(l => l.Id == request.Id, ct);

        if (entry is null)
            throw new KeyNotFoundException("Stock transfer not found.");

        var reference = entry.Reference ?? "unknown";
        var allEntries = await _context.StockLedgerEntries
            .Include(l => l.Product)
            .Include(l => l.Warehouse)
            .Where(l => l.Reference == reference && (l.Type == "TRANSFER_OUT" || l.Type == "TRANSFER_IN"))
            .ToListAsync(ct);

        var outEntry = allEntries.FirstOrDefault(e => e.Type == "TRANSFER_OUT");

        var items = allEntries.Select(e => new StockAdjustmentItemDto
        {
            Id = e.Id,
            NewQuantity = Math.Abs(e.Quantity),
            Difference = e.Quantity,
            Product = new ItemProductInfo
            {
                Name = e.Product?.Name ?? "",
                Sku = e.Product?.SKU ?? "",
                ImageUrl = e.Product?.ImageUrl,
            },
            Warehouse = new ItemWarehouseInfo
            {
                Name = e.Warehouse?.Name ?? "",
            },
        }).ToList();

        return new StockTransferDto
        {
            Id = entry.Id,
            ReferenceNumber = reference,
            TotalItems = allEntries.Select(i => i.ProductId).Distinct().Count(),
            AdjustmentDate = allEntries.Min(e => e.CreatedAt),
            Notes = outEntry?.Note,
            CreatedAt = allEntries.Min(e => e.CreatedAt),
            Items = items,
        };
    }
}
