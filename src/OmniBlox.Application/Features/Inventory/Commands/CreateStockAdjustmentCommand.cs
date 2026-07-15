using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Inventory.DTOs;
using Inv = OmniBlox.Domain.Entities.Inventory;
using StockLedgerEntry = OmniBlox.Domain.Entities.StockLedgerEntry;
using StockAdjustment = OmniBlox.Domain.Entities.StockAdjustment;
using StockAdjustmentItem = OmniBlox.Domain.Entities.StockAdjustmentItem;

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
    public CreateStockAdjustmentCommandHandler(IApplicationDbContext context) => _context = context;

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
                .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.WarehouseId == item.WarehouseId, ct);

            int previousQty = inventory?.Quantity ?? 0;
            int difference = item.NewQuantity - previousQty;

            if (inventory is null)
            {
                inventory = new Inv
                {
                    ProductId = item.ProductId,
                    WarehouseId = item.WarehouseId,
                    Quantity = item.NewQuantity,
                };
                _context.Inventories.Add(inventory);
            }
            else
            {
                inventory.Quantity = item.NewQuantity;
                inventory.UpdatedAt = DateTime.UtcNow;
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId, ct);
            if (product is not null) product.Stock += difference;

            _context.StockAdjustmentItems.Add(new StockAdjustmentItem
            {
                StockAdjustmentId = adjustment.Id,
                ProductId = item.ProductId,
                WarehouseId = item.WarehouseId,
                PreviousQuantity = previousQty,
                NewQuantity = item.NewQuantity,
                Difference = difference,
            });

            _context.StockLedgerEntries.Add(new StockLedgerEntry
            {
                ProductId = item.ProductId,
                WarehouseId = item.WarehouseId,
                Quantity = difference,
                Balance = item.NewQuantity,
                Type = request.Type == "ADDITION" ? "ADJUSTMENT_ADD" : "ADJUSTMENT_REMOVE",
                Reference = adjustment.ReferenceNumber,
                Note = request.Notes,
            });

            var warehouse = await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == item.WarehouseId, ct);
            items.Add(new StockAdjustmentItemDto
            {
                Id = Guid.Empty,
                PreviousQuantity = previousQty,
                NewQuantity = item.NewQuantity,
                Difference = difference,
                Product = new ItemProductInfo
                {
                    Name = product?.Name ?? "",
                    Sku = product?.SKU ?? "",
                    ImageUrl = product?.ImageUrl,
                },
                Warehouse = new ItemWarehouseInfo
                {
                    Name = warehouse?.Name ?? "",
                },
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
