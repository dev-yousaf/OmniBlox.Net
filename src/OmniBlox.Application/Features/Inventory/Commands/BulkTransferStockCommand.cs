using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Inventory.DTOs;

namespace OmniBlox.Application.Features.Inventory.Commands;

public record BulkTransferStockCommand : IRequest<StockTransferDto>
{
    public Guid FromWarehouseId { get; init; }
    public Guid ToWarehouseId { get; init; }
    public List<BulkTransferItem> Items { get; init; } = [];
    public string? Note { get; init; }
}

public class BulkTransferStockCommandHandler : IRequestHandler<BulkTransferStockCommand, StockTransferDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IStockService _stockService;

    public BulkTransferStockCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IStockService stockService)
    {
        _context = context;
        _currentUser = currentUser;
        _stockService = stockService;
    }

    public async Task<StockTransferDto> Handle(BulkTransferStockCommand request, CancellationToken ct)
    {
        if (request.FromWarehouseId == request.ToWarehouseId)
            throw new InvalidOperationException("Source and destination warehouses must be different.");

        var fromWh = await _context.Warehouses.AsTracking().FirstOrDefaultAsync(w => w.Id == request.FromWarehouseId, ct);
        var toWh = await _context.Warehouses.AsTracking().FirstOrDefaultAsync(w => w.Id == request.ToWarehouseId, ct);

        var note = request.Note ?? $"Bulk transfer from {fromWh?.Name} to {toWh?.Name}";
        var transferRefId = Guid.NewGuid();
        var firstOutMovementId = Guid.Empty;
        var allItems = new List<StockAdjustmentItemDto>();

        foreach (var item in request.Items)
        {
            var (outMovement, inMovement) = await _stockService.RecordTransferAsync(new RecordTransferArgs
            {
                ProductId = item.ProductId,
                FromWarehouseId = request.FromWarehouseId,
                ToWarehouseId = request.ToWarehouseId,
                Quantity = item.Quantity,
                ReferenceType = "transfer",
                ReferenceId = transferRefId,
                UserId = _currentUser.UserId,
            }, ct);

            if (firstOutMovementId == Guid.Empty)
                firstOutMovementId = outMovement.Id;

            var product = await _context.Products.AsTracking().FirstOrDefaultAsync(p => p.Id == item.ProductId, ct);
            allItems.Add(new StockAdjustmentItemDto
            {
                Id = outMovement.Id,
                NewQuantity = item.Quantity,
                Difference = -item.Quantity,
                Product = new ItemProductInfo { Name = product?.Name ?? "", Sku = product?.SKU ?? "", ImageUrl = product?.ImageUrl },
                Warehouse = new ItemWarehouseInfo { Name = fromWh?.Name ?? "" },
            });
            allItems.Add(new StockAdjustmentItemDto
            {
                Id = inMovement.Id,
                NewQuantity = item.Quantity,
                Difference = item.Quantity,
                Product = new ItemProductInfo { Name = product?.Name ?? "", Sku = product?.SKU ?? "", ImageUrl = product?.ImageUrl },
                Warehouse = new ItemWarehouseInfo { Name = toWh?.Name ?? "" },
            });
        }

        await _context.SaveChangesAsync(ct);

        return new StockTransferDto
        {
            Id = firstOutMovementId,
            ReferenceNumber = note,
            TotalItems = request.Items.Count,
            AdjustmentDate = DateTime.UtcNow,
            Notes = request.Note,
            CreatedAt = DateTime.UtcNow,
            Items = allItems,
        };
    }
}

public class BulkTransferStockCommandValidator : AbstractValidator<BulkTransferStockCommand>
{
    public BulkTransferStockCommandValidator()
    {
        RuleFor(v => v.FromWarehouseId).NotEmpty();
        RuleFor(v => v.ToWarehouseId).NotEmpty();
        RuleFor(v => v.Items).NotEmpty();
    }
}
