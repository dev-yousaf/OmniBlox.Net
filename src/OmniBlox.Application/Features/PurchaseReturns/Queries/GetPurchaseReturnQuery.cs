using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.PurchaseReturns.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.PurchaseReturns.Queries;

public record GetPurchaseReturnQuery : IRequest<PurchaseReturnDetailDto>
{
    public Guid Id { get; init; }
}

public class GetPurchaseReturnQueryHandler : IRequestHandler<GetPurchaseReturnQuery, PurchaseReturnDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetPurchaseReturnQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PurchaseReturnDetailDto> Handle(GetPurchaseReturnQuery request, CancellationToken ct)
    {
        var returnEntity = await _context.PurchaseReturns
            .Include(r => r.Warehouse)
            .Include(r => r.Supplier)
            .Include(r => r.PurchaseOrder)
            .Include(r => r.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.CompanyId == _currentUser.CompanyId, ct);
        if (returnEntity is null) throw new NotFoundException(nameof(PurchaseReturn), request.Id);

        return new PurchaseReturnDetailDto
        {
            Id = returnEntity.Id,
            ReferenceNumber = returnEntity.ReferenceNumber,
            TotalAmount = returnEntity.TotalAmount,
            Reason = returnEntity.Reason,
            Status = returnEntity.Status,
            ReturnDate = returnEntity.ReturnDate,
            WarehouseId = returnEntity.WarehouseId,
            WarehouseName = returnEntity.Warehouse?.Name ?? "",
            SupplierId = returnEntity.SupplierId,
            SupplierName = returnEntity.Supplier?.Name ?? "",
            PurchaseOrderId = returnEntity.PurchaseOrderId,
            PurchaseOrderReference = returnEntity.PurchaseOrder?.ReferenceNumber,
            CreatedAt = returnEntity.CreatedAt,
            Items = returnEntity.Items.Select(i => new PurchaseReturnItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product?.Name,
                ProductSku = i.Product?.SKU,
                Quantity = i.Quantity,
                UnitCost = i.UnitCost,
                Total = i.Quantity * i.UnitCost,
                PurchaseOrderItemId = i.PurchaseOrderItemId,
            }).ToList(),
        };
    }
}
