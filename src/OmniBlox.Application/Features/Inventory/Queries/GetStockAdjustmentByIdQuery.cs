using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Inventory.DTOs;

namespace OmniBlox.Application.Features.Inventory.Queries;

public record GetStockAdjustmentByIdQuery : IRequest<StockAdjustmentDto>
{
    public Guid Id { get; init; }
}

public class GetStockAdjustmentByIdQueryHandler : IRequestHandler<GetStockAdjustmentByIdQuery, StockAdjustmentDto>
{
    private readonly IApplicationDbContext _context;
    public GetStockAdjustmentByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<StockAdjustmentDto> Handle(GetStockAdjustmentByIdQuery request, CancellationToken ct)
    {
        var adjustment = await _context.StockAdjustments
            .Include(a => a.Items)
                .ThenInclude(i => i.Product)
            .Include(a => a.Items)
                .ThenInclude(i => i.Warehouse)
            .FirstOrDefaultAsync(a => a.Id == request.Id, ct);

        if (adjustment is null)
            throw new KeyNotFoundException("Stock adjustment not found.");

        return new StockAdjustmentDto
        {
            Id = adjustment.Id,
            ReferenceNumber = adjustment.ReferenceNumber,
            Type = adjustment.Type,
            Notes = adjustment.Notes,
            TotalItems = adjustment.TotalItems,
            NetChange = adjustment.NetChange,
            AdjustmentDate = adjustment.AdjustmentDate,
            CreatedAt = adjustment.CreatedAt,
            Items = adjustment.Items.Select(i => new StockAdjustmentItemDto
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
        };
    }
}
