using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Sales.DTOs;

namespace OmniBlox.Application.Features.Sales.Queries;

public record GetSalesStatsQuery : IRequest<SalesStatsDto>;

public class GetSalesStatsQueryHandler : IRequestHandler<GetSalesStatsQuery, SalesStatsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetSalesStatsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SalesStatsDto> Handle(GetSalesStatsQuery request, CancellationToken ct)
    {
        var sales = await _context.Sales.Where(s => s.CompanyId == _currentUser.CompanyId).ToListAsync(ct);

        var paid = sales.Where(s => s.PaymentStatus == "PAID").ToList();
        var pending = sales.Where(s => s.PaymentStatus == "PENDING").ToList();
        var overdue = sales.Where(s => s.PaymentStatus == "PENDING" && s.DueDate < DateTime.UtcNow).ToList();

        return new SalesStatsDto
        {
            TotalSales = sales.Count,
            TotalRevenue = paid.Sum(s => s.TotalAmount),
            PendingAmount = pending.Sum(s => s.TotalAmount),
            OverdueAmount = overdue.Sum(s => s.TotalAmount),
            PaidInvoices = paid.Count,
            PendingInvoices = pending.Count,
            OverdueInvoices = overdue.Count,
        };
    }
}
