using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Inventory.DTOs;
using OmniBlox.Domain.Enums;

namespace OmniBlox.Application.Features.Inventory.Commands;

public record TransferStockCommand : IRequest<StockTransferDto>
{
    public Guid ProductId { get; init; }
    public Guid FromWarehouseId { get; init; }
    public Guid ToWarehouseId { get; init; }
    public int Quantity { get; init; }
    public string? Note { get; init; }
}

public class TransferStockCommandHandler : IRequestHandler<TransferStockCommand, StockTransferDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IStockService _stockService;

    public TransferStockCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IStockService stockService)
    {
        _context = context;
        _currentUser = currentUser;
        _stockService = stockService;
    }

    public async Task<StockTransferDto> Handle(TransferStockCommand request, CancellationToken ct)
    {
        if (request.FromWarehouseId == request.ToWarehouseId)
            throw new InvalidOperationException("Source and destination warehouses must be different.");

        var fromWh = await _context.Warehouses.AsTracking().FirstOrDefaultAsync(w => w.Id == request.FromWarehouseId, ct);
        var toWh = await _context.Warehouses.AsTracking().FirstOrDefaultAsync(w => w.Id == request.ToWarehouseId, ct);

        var transferRefId = Guid.NewGuid();
        var (outMovement, inMovement) = await _stockService.RecordTransferAsync(new RecordTransferArgs
        {
            ProductId = request.ProductId,
            FromWarehouseId = request.FromWarehouseId,
            ToWarehouseId = request.ToWarehouseId,
            Quantity = request.Quantity,
            ReferenceType = "transfer",
            ReferenceId = transferRefId,
            UserId = _currentUser.UserId,
        }, ct);

        await _context.SaveChangesAsync(ct);

        var product = await _context.Products.AsTracking().FirstOrDefaultAsync(p => p.Id == request.ProductId, ct);

        return new StockTransferDto
        {
            Id = outMovement.Id,
            ReferenceNumber = request.Note ?? $"Transfer from {fromWh?.Name} to {toWh?.Name}",
            TotalItems = 1,
            AdjustmentDate = DateTime.UtcNow,
            Notes = request.Note,
            CreatedAt = DateTime.UtcNow,
            Items = new List<StockAdjustmentItemDto>
            {
                new()
                {
                    Id = outMovement.Id,
                    NewQuantity = request.Quantity,
                    Difference = -request.Quantity,
                    Product = new ItemProductInfo { Name = product?.Name ?? "", Sku = product?.SKU ?? "", ImageUrl = product?.ImageUrl },
                    Warehouse = new ItemWarehouseInfo { Name = fromWh?.Name ?? "" },
                },
                new()
                {
                    Id = inMovement.Id,
                    NewQuantity = request.Quantity,
                    Difference = request.Quantity,
                    Product = new ItemProductInfo { Name = product?.Name ?? "", Sku = product?.SKU ?? "", ImageUrl = product?.ImageUrl },
                    Warehouse = new ItemWarehouseInfo { Name = toWh?.Name ?? "" },
                },
            },
        };
    }
}

public class TransferStockCommandValidator : AbstractValidator<TransferStockCommand>
{
    public TransferStockCommandValidator()
    {
        RuleFor(v => v.ProductId).NotEmpty();
        RuleFor(v => v.FromWarehouseId).NotEmpty();
        RuleFor(v => v.ToWarehouseId).NotEmpty();
        RuleFor(v => v.Quantity).GreaterThan(0);
    }
}
