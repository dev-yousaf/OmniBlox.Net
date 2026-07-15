using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Inventory.DTOs;

namespace OmniBlox.Application.Features.Inventory.Queries;

public record GetStockTransfersQuery : IRequest<TransferListResponse>
{
    public int Page { get; init; } = 1;
    public int Limit { get; init; } = 20;
}

public class GetStockTransfersQueryHandler : IRequestHandler<GetStockTransfersQuery, TransferListResponse>
{
    private readonly IApplicationDbContext _context;
    public GetStockTransfersQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<TransferListResponse> Handle(GetStockTransfersQuery request, CancellationToken ct)
    {
        var entries = await _context.StockLedgerEntries
            .Include(l => l.Product)
            .Include(l => l.Warehouse)
            .Where(l => l.Type == "TRANSFER_OUT" || l.Type == "TRANSFER_IN")
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(ct);

        var groups = entries
            .GroupBy(l => l.Reference ?? "unknown")
            .Select(g =>
            {
                var outEntry = g.FirstOrDefault(e => e.Type == "TRANSFER_OUT");
                var inEntries = g.Where(e => e.Type == "TRANSFER_IN").ToList();
                var allItems = g.ToList();

                var items = allItems.Select(e => new StockAdjustmentItemDto
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
                    Id = g.First().Id,
                    ReferenceNumber = g.Key,
                    TotalItems = allItems.Select(i => i.ProductId).Distinct().Count(),
                    AdjustmentDate = g.Min(e => e.CreatedAt),
                    Notes = outEntry?.Note,
                    CreatedAt = g.Min(e => e.CreatedAt),
                    Items = items,
                };
            })
            .ToList();

        var total = groups.Count;
        var pages = (int)Math.Ceiling(total / (double)request.Limit);
        var paged = groups
            .Skip((request.Page - 1) * request.Limit)
            .Take(request.Limit)
            .ToList();

        return new TransferListResponse
        {
            Transfers = paged,
            Total = total,
            Pages = pages,
        };
    }
}
