using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Products.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Products.Commands;

public record UpdateStockCommand : IRequest<ProductDto>
{
    public Guid Id { get; init; }
    public int Quantity { get; init; }
    public string Operation { get; init; } = "add";
    public Guid? WarehouseId { get; init; }
}

public class UpdateStockCommandHandler : IRequestHandler<UpdateStockCommand, ProductDto>
{
    private readonly IApplicationDbContext _context;
    public UpdateStockCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<ProductDto> Handle(UpdateStockCommand request, CancellationToken ct)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.Id, ct);
        if (product is null) throw new NotFoundException(nameof(Product), request.Id);

        var delta = request.Operation.ToLower() == "add" ? request.Quantity : -request.Quantity;

        product.Stock += delta;
        if (product.Stock < 0) product.Stock = 0;
        product.UpdatedAt = DateTime.UtcNow;

        // Also update warehouse-specific inventory
        var warehouseId = request.WarehouseId;
        if (warehouseId is null)
        {
            warehouseId = await _context.Inventories
                .Where(i => i.ProductId == product.Id)
                .Select(i => (Guid?)i.WarehouseId)
                .FirstOrDefaultAsync(ct);
        }

        if (warehouseId.HasValue)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.ProductId == product.Id && i.WarehouseId == warehouseId.Value, ct);

            if (inventory is null)
            {
                inventory = new Domain.Entities.Inventory
                {
                    ProductId = product.Id,
                    WarehouseId = warehouseId.Value,
                    Quantity = 0,
                };
                _context.Inventories.Add(inventory);
            }

            inventory.Quantity += delta;
            if (inventory.Quantity < 0) inventory.Quantity = 0;
            inventory.UpdatedAt = DateTime.UtcNow;

            product.Stock += delta;
        }

        _context.StockLedgerEntries.Add(new StockLedgerEntry
        {
            ProductId = product.Id,
            WarehouseId = warehouseId,
            Quantity = delta,
            Balance = product.Stock,
            Type = request.Operation.ToUpper(),
            Reference = $"Stock {request.Operation}",
        });

        await _context.SaveChangesAsync(ct);
        return ProductDto.FromEntity(product);
    }
}
