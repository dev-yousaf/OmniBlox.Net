using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Inventory.DTOs;
using OmniBlox.Domain.Enums;

namespace OmniBlox.Application.Features.Inventory.Queries;

public record GetStockTransferByIdQuery : IRequest<StockTransferDto>
{
    public Guid Id { get; init; }
}

public class GetStockTransferByIdQueryHandler : IRequestHandler<GetStockTransferByIdQuery, StockTransferDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetStockTransferByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<StockTransferDto> Handle(GetStockTransferByIdQuery request, CancellationToken ct)
    {
        var movement = await _context.StockMovements
            .Include(m => m.Product)
            .Include(m => m.Warehouse)
            .FirstOrDefaultAsync(m => m.Id == request.Id && m.Product!.CompanyId == _currentUser.CompanyId, ct);

        if (movement is null)
            throw new KeyNotFoundException("Stock transfer not found.");

        var referenceId = movement.ReferenceId ?? Guid.Empty;
        var allMovements = await _context.StockMovements
            .Include(m => m.Product)
            .Include(m => m.Warehouse)
            .Where(m => m.ReferenceId == referenceId && m.Product!.CompanyId == _currentUser.CompanyId
                && (m.MovementType == MovementType.transfer_out || m.MovementType == MovementType.transfer_in))
            .ToListAsync(ct);

        var outMovement = allMovements.FirstOrDefault(e => e.MovementType == MovementType.transfer_out);

        var items = allMovements.Select(e => new StockAdjustmentItemDto
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
            Id = movement.Id,
            ReferenceNumber = $"TRF-{referenceId.ToString("N")[..8].ToUpperInvariant()}",
            TotalItems = allMovements.Select(i => i.ProductId).Distinct().Count(),
            AdjustmentDate = allMovements.Min(e => e.CreatedAt),
            Notes = null,
            CreatedAt = allMovements.Min(e => e.CreatedAt),
            Items = items,
        };
    }
}
