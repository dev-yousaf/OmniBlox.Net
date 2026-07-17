using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Inventory.DTOs;

namespace OmniBlox.Application.Features.Inventory.Queries;

public record GetInventoryByProductQuery : IRequest<List<InventoryDto>>
{
    public Guid ProductId { get; init; }
}

public class GetInventoryByProductQueryHandler : IRequestHandler<GetInventoryByProductQuery, List<InventoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetInventoryByProductQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<InventoryDto>> Handle(GetInventoryByProductQuery request, CancellationToken ct)
    {
        var items = await _context.Inventories
            .Include(i => i.Product)
            .Include(i => i.Warehouse)
            .Where(i => i.ProductId == request.ProductId)
            .OrderBy(i => i.Warehouse.Name)
            .ToListAsync(ct);

        if (items.Count > 0)
            return items.Select(InventoryDto.FromEntity).ToList();

        return [];
    }
}
