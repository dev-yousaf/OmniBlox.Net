using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Purchases.DTOs;

namespace OmniBlox.Application.Features.Purchases.Queries;

public record GetPurchaseStatsQuery : IRequest<PurchaseOrderStatsDto>;

public class GetPurchaseStatsQueryHandler : IRequestHandler<GetPurchaseStatsQuery, PurchaseOrderStatsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetPurchaseStatsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PurchaseOrderStatsDto> Handle(GetPurchaseStatsQuery request, CancellationToken ct)
    {
        var totalPurchases = await _context.PurchaseOrders
            .Where(o => o.CompanyId == _currentUser.CompanyId)
            .CountAsync(ct);
        var totalCost = await _context.PurchaseOrders
            .Where(o => o.CompanyId == _currentUser.CompanyId)
            .SumAsync(o => (decimal?)o.TotalAmount ?? 0, ct);

        var pendingAmount = await _context.PurchaseOrders
            .Where(o => o.CompanyId == _currentUser.CompanyId)
            .Where(o => o.PaymentStatus == "PENDING")
            .SumAsync(o => (decimal?)o.TotalAmount ?? 0, ct);

        var paidAmount = await _context.PurchaseOrders
            .Where(o => o.CompanyId == _currentUser.CompanyId)
            .Where(o => o.PaymentStatus == "PAID")
            .SumAsync(o => (decimal?)o.TotalAmount ?? 0, ct);

        var pendingCount = await _context.PurchaseOrders
            .Where(o => o.CompanyId == _currentUser.CompanyId)
            .Where(o => o.PaymentStatus == "PENDING")
            .CountAsync(ct);

        var receivedCount = await _context.PurchaseOrders
            .Where(o => o.CompanyId == _currentUser.CompanyId)
            .Where(o => o.Status == "COMPLETED")
            .CountAsync(ct);

        var now = DateTime.UtcNow;
        var overdueAmount = await _context.PurchaseOrders
            .Where(o => o.CompanyId == _currentUser.CompanyId)
            .Where(o => o.DueDate != null && o.DueDate < now && o.PaymentStatus != "PAID")
            .SumAsync(o => (decimal?)o.TotalAmount ?? 0, ct);

        return new PurchaseOrderStatsDto
        {
            TotalPurchases = totalPurchases,
            TotalCost = totalCost,
            PendingAmount = pendingAmount,
            PaidAmount = paidAmount,
            PendingCount = pendingCount,
            ReceivedCount = receivedCount,
            OverdueAmount = overdueAmount,
        };
    }
}
