using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Inventory.DTOs;
using OmniBlox.Domain.Enums;

namespace OmniBlox.Application.Features.Inventory.Queries;

public record GetStockTransfersQuery : IRequest<TransferListResponse>
{
    public int Page { get; init; } = 1;
    public int Limit { get; init; } = 20;
}

public class GetStockTransfersQueryHandler : IRequestHandler<GetStockTransfersQuery, TransferListResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetStockTransfersQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<TransferListResponse> Handle(GetStockTransfersQuery request, CancellationToken ct)
    {
        var movements = await _context.StockMovements
            .Include(m => m.Product)
            .Include(m => m.Warehouse)
            .Where(m => m.Product!.CompanyId == _currentUser.CompanyId)
            .Where(m => m.MovementType == MovementType.transfer_out || m.MovementType == MovementType.transfer_in)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(ct);

        var groups = movements
            .GroupBy(m => m.ReferenceId ?? Guid.Empty)
            .Select(g =>
            {
                var outMovement = g.FirstOrDefault(e => e.MovementType == MovementType.transfer_out);

                var items = g.Select(e => new StockAdjustmentItemDto
                {
                    Id = e.Id,
                    NewQuantity = e.Quantity,
                    Difference = e.MovementType == MovementType.transfer_out ? -e.Quantity : e.Quantity,
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
                    ReferenceNumber = $"TRF-{g.Key.ToString("N")[..8].ToUpperInvariant()}",
                    TotalItems = g.Select(i => i.ProductId).Distinct().Count(),
                    AdjustmentDate = g.Min(e => e.CreatedAt),
                    Notes = null,
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
