using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Products.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Products.Queries;

public record GetProductQuery : IRequest<ProductDto>
{
    public Guid Id { get; init; }
}

public class GetProductQueryHandler : IRequestHandler<GetProductQuery, ProductDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetProductQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ProductDto> Handle(GetProductQuery request, CancellationToken ct)
    {
        var product = await _context.Products
            .Include(p => p.CreatedByUser)
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.CompanyId == _currentUser.CompanyId, ct);

        if (product is null)
            throw new NotFoundException(nameof(Product), request.Id);

        var warehouseId = await _context.Inventories
            .Where(i => i.ProductId == product.Id)
            .Select(i => (Guid?)i.WarehouseId)
            .FirstOrDefaultAsync(ct);

        // Compute live total stock from warehouse_stock
        var liveStock = await _context.Inventories
            .Where(i => i.ProductId == product.Id)
            .SumAsync(i => (int?)i.Quantity ?? 0, ct);
        product.Stock = liveStock;

        return ProductDto.FromEntity(product, warehouseId);
    }
}
