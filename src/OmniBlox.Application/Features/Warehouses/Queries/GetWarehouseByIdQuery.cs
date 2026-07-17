using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Warehouses.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Warehouses.Queries;

public record GetWarehouseByIdQuery : IRequest<WarehouseDto>
{
    public Guid Id { get; init; }
}

public class GetWarehouseByIdQueryHandler : IRequestHandler<GetWarehouseByIdQuery, WarehouseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetWarehouseByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<WarehouseDto> Handle(GetWarehouseByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.Warehouses
            .Include(w => w.Inventories)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == _currentUser.CompanyId, ct);
        if (entity is null) throw new NotFoundException(nameof(Warehouse), request.Id);
        var dto = WarehouseDto.FromEntity(entity);
        var inventory = entity.Inventories.Select(i => new WarehouseInventoryItemDto
        {
            Quantity = i.Quantity,
            Product = new WarehouseProductInfo
            {
                Id = i.Product.Id,
                Name = i.Product.Name,
                Sku = i.Product.SKU,
                SalePrice = i.Product.SalePrice,
                Category = string.IsNullOrEmpty(i.Product.Category) ? null : new WarehouseCategoryInfo { Name = i.Product.Category },
                Brand = string.IsNullOrEmpty(i.Product.Brand) ? null : new WarehouseBrandInfo { Name = i.Product.Brand },
            },
        }).ToList();
        return dto with
        {
            Count = new WarehouseCount { ProductCount = inventory.Count },
            Inventory = inventory.Count > 0 ? inventory : null,
        };
    }
}
