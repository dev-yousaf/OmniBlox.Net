using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Dashboard.DTOs;

namespace OmniBlox.Application.Features.Dashboard.Queries;

public record GetSalesStatsQuery : IRequest<SalesStatsResponseDto>
{
    public string Period { get; init; } = "1Y";
}

public class GetSalesStatsQueryHandler : IRequestHandler<GetSalesStatsQuery, SalesStatsResponseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetSalesStatsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SalesStatsResponseDto> Handle(GetSalesStatsQuery request, CancellationToken ct)
    {
        var companyId = _currentUser.CompanyId;
        var now = DateTime.UtcNow;
        var (fromDate, prevFromDate) = GetDateRange(request.Period, now);

        var sales = await _context.Sales
            .Where(s => s.CompanyId == companyId && s.SaleDate >= fromDate)
            .ToListAsync(ct);
        var prevSales = await _context.Sales
            .Where(s => s.CompanyId == companyId && s.SaleDate >= prevFromDate && s.SaleDate < fromDate)
            .ToListAsync(ct);
        var expenses = await _context.Expenses
            .Where(e => e.CompanyId == companyId && e.ExpenseDate >= fromDate)
            .ToListAsync(ct);
        var prevExpenses = await _context.Expenses
            .Where(e => e.CompanyId == companyId && e.ExpenseDate >= prevFromDate && e.ExpenseDate < fromDate)
            .ToListAsync(ct);
        var purchases = await _context.PurchaseOrders
            .Where(p => p.CompanyId == companyId && p.OrderDate >= fromDate)
            .ToListAsync(ct);

        var totalRevenue = sales.Sum(s => s.TotalAmount);
        var prevRevenue = prevSales.Sum(s => s.TotalAmount);
        var totalExpenses = expenses.Sum(e => e.Amount);
        var prevExpensesTotal = prevExpenses.Sum(e => e.Amount);

        var chartData = BuildChartData(sales, purchases, fromDate, now);

        return new SalesStatsResponseDto
        {
            Chart = chartData,
            TotalRevenue = totalRevenue,
            TotalExpenses = totalExpenses,
            RevenueChange = prevRevenue > 0 ? Math.Round((totalRevenue - prevRevenue) / prevRevenue * 100, 1) : 0,
            ExpenseChange = prevExpensesTotal > 0 ? Math.Round((totalExpenses - prevExpensesTotal) / prevExpensesTotal * 100, 1) : 0,
        };
    }

    private static (DateTime from, DateTime prevFrom) GetDateRange(string period, DateTime now)
    {
        var from = period switch
        {
            "1D" => now.AddDays(-1),
            "1W" => now.AddDays(-7),
            "1M" => now.AddMonths(-1),
            "3M" => now.AddMonths(-3),
            "6M" => now.AddMonths(-6),
            _ => now.AddYears(-1),
        };
        var range = now - from;
        var prevFrom = from - range;
        return (from, prevFrom);
    }

    private static List<SalesPurchaseChartItemDto> BuildChartData(
        List<Domain.Entities.Sale> sales,
        List<Domain.Entities.PurchaseOrder> purchases,
        DateTime fromDate, DateTime now)
    {
        var months = new List<SalesPurchaseChartItemDto>();
        var start = new DateTime(fromDate.Year, fromDate.Month, 1);
        var end = new DateTime(now.Year, now.Month, 1);

        for (var m = start; m <= end; m = m.AddMonths(1))
        {
            months.Add(new SalesPurchaseChartItemDto
            {
                Month = m.ToString("MMM yyyy"),
                Sales = sales.Where(s => s.SaleDate.Year == m.Year && s.SaleDate.Month == m.Month).Sum(s => s.TotalAmount),
                Purchase = purchases.Where(p => p.OrderDate.Year == m.Year && p.OrderDate.Month == m.Month).Sum(p => p.TotalAmount),
            });
        }

        return months;
    }
}
