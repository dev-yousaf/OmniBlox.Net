using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.PurchaseReturns.DTOs;

namespace OmniBlox.Application.Features.PurchaseReturns.Queries;

public record GetPurchaseReturnsQuery : IRequest<PurchaseReturnListResponse>
{
    public string? Search { get; init; }
    public string? Status { get; init; }
    public Guid? SupplierId { get; init; }
    public Guid? WarehouseId { get; init; }
    public Guid? PurchaseOrderId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public int Page { get; init; } = 1;
    public int Limit { get; init; } = 20;
}

public class GetPurchaseReturnsQueryHandler : IRequestHandler<GetPurchaseReturnsQuery, PurchaseReturnListResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetPurchaseReturnsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PurchaseReturnListResponse> Handle(GetPurchaseReturnsQuery request, CancellationToken ct)
    {
        var query = _context.PurchaseReturns
            .Where(e => e.CompanyId == _currentUser.CompanyId)
            .Include(r => r.Warehouse)
            .Include(r => r.Supplier)
            .Include(r => r.PurchaseOrder)
            .Include(r => r.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.ToLower();
            query = query.Where(r =>
                r.ReferenceNumber.ToLower().Contains(s) ||
                r.Supplier.Name.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(r => r.Status == request.Status);

        if (request.SupplierId.HasValue)
            query = query.Where(r => r.SupplierId == request.SupplierId.Value);

        if (request.WarehouseId.HasValue)
            query = query.Where(r => r.WarehouseId == request.WarehouseId.Value);

        if (request.PurchaseOrderId.HasValue)
            query = query.Where(r => r.PurchaseOrderId == request.PurchaseOrderId.Value);

        if (request.FromDate.HasValue)
            query = query.Where(r => r.ReturnDate >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(r => r.ReturnDate <= request.ToDate.Value);

        var total = await query.CountAsync(ct);
        var pages = (int)Math.Ceiling(total / (double)request.Limit);

        var returns = await query
            .OrderByDescending(r => r.ReturnDate)
            .Skip((request.Page - 1) * request.Limit)
            .Take(request.Limit)
            .ToListAsync(ct);

        return new PurchaseReturnListResponse
        {
            Returns = returns.Select(r => new PurchaseReturnSummaryDto
            {
                Id = r.Id,
                ReferenceNumber = r.ReferenceNumber,
                TotalAmount = r.TotalAmount,
                Reason = r.Reason,
                Status = r.Status,
                ReturnDate = r.ReturnDate,
                WarehouseId = r.WarehouseId,
                WarehouseName = r.Warehouse?.Name ?? "",
                SupplierId = r.SupplierId,
                SupplierName = r.Supplier?.Name ?? "",
                PurchaseOrderId = r.PurchaseOrderId,
                PurchaseOrderReference = r.PurchaseOrder?.ReferenceNumber,
                ItemCount = r.Items.Count,
                CreatedAt = r.CreatedAt,
            }).ToList(),
            Total = total,
            Pages = pages,
        };
    }
}
