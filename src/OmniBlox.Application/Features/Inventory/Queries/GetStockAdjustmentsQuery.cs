using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Inventory.DTOs;

namespace OmniBlox.Application.Features.Inventory.Queries;

public record GetStockAdjustmentsQuery : IRequest<AdjustmentListResponse>
{
    public int Page { get; init; } = 1;
    public int Limit { get; init; } = 10;
}

public class GetStockAdjustmentsQueryHandler : IRequestHandler<GetStockAdjustmentsQuery, AdjustmentListResponse>
{
    private readonly IApplicationDbContext _context;
    public GetStockAdjustmentsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<AdjustmentListResponse> Handle(GetStockAdjustmentsQuery request, CancellationToken ct)
    {
        var total = await _context.StockAdjustments.CountAsync(ct);
        var pages = (int)Math.Ceiling(total / (double)request.Limit);

        var adjustments = await _context.StockAdjustments
            .Include(a => a.Items)
                .ThenInclude(i => i.Product)
            .Include(a => a.Items)
                .ThenInclude(i => i.Warehouse)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.Page - 1) * request.Limit)
            .Take(request.Limit)
            .ToListAsync(ct);

        var dtos = adjustments.Select(a => new StockAdjustmentDto
        {
            Id = a.Id,
            ReferenceNumber = a.ReferenceNumber,
            Type = a.Type,
            Notes = a.Notes,
            TotalItems = a.TotalItems,
            NetChange = a.NetChange,
            AdjustmentDate = a.AdjustmentDate,
            CreatedAt = a.CreatedAt,
            Items = a.Items.Select(i => new StockAdjustmentItemDto
            {
                Id = i.Id,
                PreviousQuantity = i.PreviousQuantity,
                NewQuantity = i.NewQuantity,
                Difference = i.Difference,
                Product = new ItemProductInfo
                {
                    Name = i.Product?.Name ?? "",
                    Sku = i.Product?.SKU ?? "",
                    ImageUrl = i.Product?.ImageUrl,
                },
                Warehouse = new ItemWarehouseInfo
                {
                    Name = i.Warehouse?.Name ?? "",
                },
            }).ToList(),
        }).ToList();

        return new AdjustmentListResponse
        {
            Adjustments = dtos,
            Total = total,
            Pages = pages,
        };
    }
}
