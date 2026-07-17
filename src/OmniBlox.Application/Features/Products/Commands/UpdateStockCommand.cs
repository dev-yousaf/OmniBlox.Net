using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Products.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
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
    private readonly ICurrentUserService _currentUser;
    private readonly IStockService _stockService;

    public UpdateStockCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IStockService stockService)
    {
        _context = context;
        _currentUser = currentUser;
        _stockService = stockService;
    }

    public async Task<ProductDto> Handle(UpdateStockCommand request, CancellationToken ct)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.Id, ct);
        if (product is null) throw new NotFoundException(nameof(Product), request.Id);

        var warehouseId = request.WarehouseId;
        if (warehouseId is null)
        {
            warehouseId = await _context.Inventories
                .Where(i => i.ProductId == product.Id)
                .Select(i => (Guid?)i.WarehouseId)
                .FirstOrDefaultAsync(ct);
        }

        if (!warehouseId.HasValue)
            throw new InvalidOperationException("No warehouse found for this product.");

        var isAddition = request.Operation.ToLower() == "add";
        var movementType = isAddition ? MovementType.adjustment_in : MovementType.adjustment_out;

        await _stockService.RecordMovementAsync(new RecordMovementArgs
        {
            ProductId = product.Id,
            WarehouseId = warehouseId.Value,
            MovementType = movementType,
            Quantity = request.Quantity,
            ReferenceType = "product",
            ReferenceId = product.Id,
            UserId = _currentUser.UserId,
        }, ct);

        await _context.SaveChangesAsync(ct);
        return ProductDto.FromEntity(product);
    }
}