using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Quotations.Queries;

public record GetQuotationStockLevelsQuery : IRequest<QuotationStockLevelResponse>
{
    public Guid QuotationId { get; init; }
}

public record QuotationStockLevelResponse
{
    public required List<WarehouseStockInfo> Warehouses { get; init; }
}

public record WarehouseStockInfo
{
    public Guid WarehouseId { get; init; }
    public string WarehouseName { get; init; } = string.Empty;
    public string? Location { get; init; }
    public bool CanFulfill { get; init; }
    public required List<ProductStockInfo> Products { get; init; }
}

public record ProductStockInfo
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string? Sku { get; init; }
    public int Required { get; init; }
    public int Available { get; init; }
    public bool Sufficient { get; init; }
}

public class GetQuotationStockLevelsQueryHandler : IRequestHandler<GetQuotationStockLevelsQuery, QuotationStockLevelResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetQuotationStockLevelsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<QuotationStockLevelResponse> Handle(GetQuotationStockLevelsQuery request, CancellationToken ct)
    {
        var quotation = await _context.Quotations
            .Include(q => q.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(q => q.Id == request.QuotationId, ct);

        if (quotation is null)
            throw new NotFoundException(nameof(Quotation), request.QuotationId);

        var productIds = quotation.Items.Select(i => i.ProductId).ToList();

        var warehouses = await _context.Warehouses
            .Include(w => w.Inventories)
            .Where(w => w.CompanyId == _currentUser.CompanyId)
            .ToListAsync(ct);

        var warehouseInfoList = new List<WarehouseStockInfo>();

        foreach (var warehouse in warehouses)
        {
            var productInfoList = new List<ProductStockInfo>();
            var canFulfill = true;

            foreach (var item in quotation.Items)
            {
                var inventory = warehouse.Inventories
                    .FirstOrDefault(i => i.ProductId == item.ProductId);

                var available = inventory?.Quantity ?? 0;
                var sufficient = available >= item.Quantity;

                if (!sufficient) canFulfill = false;

                productInfoList.Add(new ProductStockInfo
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product?.Name ?? "Unknown",
                    Sku = item.Product?.SKU,
                    Required = item.Quantity,
                    Available = available,
                    Sufficient = sufficient,
                });
            }

            warehouseInfoList.Add(new WarehouseStockInfo
            {
                WarehouseId = warehouse.Id,
                WarehouseName = warehouse.Name,
                Location = warehouse.Location,
                CanFulfill = canFulfill,
                Products = productInfoList,
            });
        }

        return new QuotationStockLevelResponse
        {
            Warehouses = warehouseInfoList,
        };
    }
}
