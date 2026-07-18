using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Inventory.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;

namespace OmniBlox.Application.Features.Inventory.Commands;

public record CreateStockAdjustmentCommand : IRequest<StockAdjustmentDto>
{
    public string Type { get; init; } = "ADDITION";
    public List<AdjustmentItem> Items { get; init; } = [];
    public string? Notes { get; init; }
    public string? DocumentUrl { get; init; }
}

public class CreateStockAdjustmentCommandHandler : IRequestHandler<CreateStockAdjustmentCommand, StockAdjustmentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IStockService _stockService;

    public CreateStockAdjustmentCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IStockService stockService)
    {
        _context = context;
        _currentUser = currentUser;
        _stockService = stockService;
    }

    public async Task<StockAdjustmentDto> Handle(CreateStockAdjustmentCommand request, CancellationToken ct)
    {
        var companyId = await _context.Warehouses
            .Where(w => w.Id == request.Items.First().WarehouseId)
            .Select(w => w.CompanyId)
            .FirstOrDefaultAsync(ct);

        var adjustment = new StockAdjustment
        {
            CompanyId = companyId,
            ReferenceNumber = $"ADJ-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
            AdjustmentDate = DateTime.UtcNow,
            Type = request.Type,
            Notes = request.Notes,
            DocumentUrl = request.DocumentUrl,
        };
        _context.StockAdjustments.Add(adjustment);
        await _context.SaveChangesAsync(ct);

        int totalItems = 0;
        int netChange = 0;
        var items = new List<StockAdjustmentItemDto>();

        foreach (var item in request.Items)
        {
            var inventory = await _context.Inventories
                .AsTracking().FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.WarehouseId == item.WarehouseId, ct);

            int previousQty = inventory?.Quantity ?? 0;
            int difference = item.NewQuantity - previousQty;

            if (difference == 0)
            {
                var p0 = await _context.Products.AsTracking().FirstOrDefaultAsync(p => p.Id == item.ProductId, ct);
                var w0 = await _context.Warehouses.AsTracking().FirstOrDefaultAsync(w => w.Id == item.WarehouseId, ct);
                items.Add(new StockAdjustmentItemDto
                {
                    Id = Guid.Empty,
                    PreviousQuantity = previousQty,
                    NewQuantity = item.NewQuantity,
                    Difference = 0,
                    Product = new ItemProductInfo { Name = p0?.Name ?? "", Sku = p0?.SKU ?? "", ImageUrl = p0?.ImageUrl },
                    Warehouse = new ItemWarehouseInfo { Name = w0?.Name ?? "" },
                });
                totalItems++;
                continue;
            }

            var movementType = difference > 0 ? MovementType.adjustment_in : MovementType.adjustment_out;

            await _stockService.RecordMovementAsync(new RecordMovementArgs
            {
                ProductId = item.ProductId,
                WarehouseId = item.WarehouseId,
                MovementType = movementType,
                Quantity = Math.Abs(difference),
                ReferenceType = "adjustment",
                ReferenceId = adjustment.Id,
                UserId = _currentUser.UserId,
            }, ct);

            _context.StockAdjustmentItems.Add(new StockAdjustmentItem
            {
                StockAdjustmentId = adjustment.Id,
                ProductId = item.ProductId,
                WarehouseId = item.WarehouseId,
                PreviousQuantity = previousQty,
                NewQuantity = item.NewQuantity,
                Difference = difference,
            });

            var product = await _context.Products.AsTracking().FirstOrDefaultAsync(p => p.Id == item.ProductId, ct);
            var warehouse = await _context.Warehouses.AsTracking().FirstOrDefaultAsync(w => w.Id == item.WarehouseId, ct);
            items.Add(new StockAdjustmentItemDto
            {
                Id = Guid.Empty,
                PreviousQuantity = previousQty,
                NewQuantity = item.NewQuantity,
                Difference = difference,
                Product = new ItemProductInfo { Name = product?.Name ?? "", Sku = product?.SKU ?? "", ImageUrl = product?.ImageUrl },
                Warehouse = new ItemWarehouseInfo { Name = warehouse?.Name ?? "" },
            });

            totalItems++;
            netChange += difference;
        }

        adjustment.TotalItems = totalItems;
        adjustment.NetChange = netChange;
        await _context.SaveChangesAsync(ct);

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
            Items = items,
        };
    }
}

public class CreateStockAdjustmentCommandValidator : AbstractValidator<CreateStockAdjustmentCommand>
{
    public CreateStockAdjustmentCommandValidator()
    {
        RuleFor(v => v.Type).NotEmpty().Must(t => t == "ADDITION" || t == "REMOVAL");
        RuleFor(v => v.Items).NotEmpty();
        RuleForEach(v => v.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.WarehouseId).NotEmpty();
            item.RuleFor(i => i.NewQuantity).GreaterThanOrEqualTo(0);
        });
    }
}
