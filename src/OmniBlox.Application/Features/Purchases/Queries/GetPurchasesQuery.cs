using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Purchases.DTOs;

namespace OmniBlox.Application.Features.Purchases.Queries;

public record GetPurchasesQuery : IRequest<PurchaseListResponse>
{
    public string? Search { get; init; }
    public string? Status { get; init; }
    public string? PaymentStatus { get; init; }
    public Guid? SupplierId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public int Page { get; init; } = 1;
    public int Limit { get; init; } = 20;
}

public class GetPurchasesQueryHandler : IRequestHandler<GetPurchasesQuery, PurchaseListResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetPurchasesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PurchaseListResponse> Handle(GetPurchasesQuery request, CancellationToken ct)
    {
        var query = _context.PurchaseOrders
            .Where(e => e.CompanyId == _currentUser.CompanyId)
            .Include(o => o.Supplier)
            .Include(o => o.Warehouse)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.ToLower();
            query = query.Where(o =>
                o.ReferenceNumber.ToLower().Contains(s) ||
                o.Supplier.Name.ToLower().Contains(s) ||
                o.BillNumber != null && o.BillNumber.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(o => o.Status == request.Status);

        if (!string.IsNullOrWhiteSpace(request.PaymentStatus))
            query = query.Where(o => o.PaymentStatus == request.PaymentStatus);

        if (request.SupplierId.HasValue)
            query = query.Where(o => o.SupplierId == request.SupplierId.Value);

        if (request.FromDate.HasValue)
            query = query.Where(o => o.OrderDate >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(o => o.OrderDate <= request.ToDate.Value);

        var total = await query.CountAsync(ct);
        var pages = (int)Math.Ceiling(total / (double)request.Limit);

        var orders = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((request.Page - 1) * request.Limit)
            .Take(request.Limit)
            .ToListAsync(ct);

        var orderIds = orders.Select(o => o.Id).ToList();

        var returnCounts = await _context.PurchaseReturns
            .Where(r => r.PurchaseOrderId != null && orderIds.Contains(r.PurchaseOrderId.Value))
            .GroupBy(r => r.PurchaseOrderId)
            .Select(g => new
            {
                PurchaseOrderId = g.Key!.Value,
                PendingCount = g.Count(r => r.Status == "PENDING"),
                ProcessingCount = g.Count(r => r.Status == "PROCESSING"),
                CompletedCount = g.Count(r => r.Status == "COMPLETED"),
            })
            .ToListAsync(ct);

        var returnCountsMap = returnCounts.ToDictionary(r => r.PurchaseOrderId);

        return new PurchaseListResponse
        {
            Purchases = orders.Select(o =>
            {
                var subtotal = o.Items.Sum(i => i.Quantity * i.UnitCost);
                var returnedValue = o.Items.Sum(i => i.ReturnedQuantity * i.UnitCost);
                returnCountsMap.TryGetValue(o.Id, out var rc);

                return new PurchaseOrderSummaryDto
                {
                    Id = o.Id,
                    ReferenceNumber = o.ReferenceNumber,
                    BillNumber = o.BillNumber,
                    BillDate = o.BillDate,
                    DueDate = o.DueDate,
                    PaymentStatus = o.PaymentStatus,
                    PaymentMethod = o.PaymentMethod,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    HasReturns = o.HasReturns,
                    SupplierId = o.SupplierId,
                    SupplierName = o.Supplier?.Name ?? "",
                    WarehouseId = o.WarehouseId,
                    WarehouseName = o.Warehouse?.Name,
                    Subtotal = subtotal,
                    TotalAmount = o.TotalAmount,
                    NetTotal = o.TotalAmount,
                    ReturnedValue = returnedValue,
                    ReturnStatus = o.HasReturns ? "returned" : null,
                    PendingReturnCount = rc?.PendingCount ?? 0,
                    ProcessingReturnCount = rc?.ProcessingCount ?? 0,
                    CompletedReturnCount = rc?.CompletedCount ?? 0,
                    CreatedAt = o.CreatedAt,
                };
            }).ToList(),
            Total = total,
            Pages = pages,
        };
    }
}
