using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Dashboard.DTOs;

namespace OmniBlox.Application.Features.Dashboard.Queries;

public record GetDashboardQuery : IRequest<DashboardDataDto>
{
    public string Period { get; init; } = "1Y";
}

public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardDataDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetDashboardQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<DashboardDataDto> Handle(GetDashboardQuery request, CancellationToken ct)
    {
        var companyId = _currentUser.CompanyId;
        var now = DateTime.UtcNow;
        var (fromDate, prevFromDate) = GetDateRange(request.Period, now);

        var salesTask = _context.Sales
            .Where(s => s.CompanyId == companyId && s.SaleDate >= fromDate)
            .ToListAsync(ct);
        var prevSalesTask = _context.Sales
            .Where(s => s.CompanyId == companyId && s.SaleDate >= prevFromDate && s.SaleDate < fromDate)
            .ToListAsync(ct);
        var purchasesTask = _context.PurchaseOrders
            .Where(p => p.CompanyId == companyId && p.OrderDate >= fromDate)
            .ToListAsync(ct);
        var prevPurchasesTask = _context.PurchaseOrders
            .Where(p => p.CompanyId == companyId && p.OrderDate >= prevFromDate && p.OrderDate < fromDate)
            .ToListAsync(ct);
        var salesReturnsTask = _context.SalesReturns
            .Where(sr => sr.CompanyId == companyId && sr.ReturnDate >= fromDate)
            .ToListAsync(ct);
        var prevSalesReturnsTask = _context.SalesReturns
            .Where(sr => sr.CompanyId == companyId && sr.ReturnDate >= prevFromDate && sr.ReturnDate < fromDate)
            .ToListAsync(ct);
        var purchaseReturnsTask = _context.PurchaseReturns
            .Where(pr => pr.CompanyId == companyId && pr.ReturnDate >= fromDate)
            .ToListAsync(ct);
        var prevPurchaseReturnsTask = _context.PurchaseReturns
            .Where(pr => pr.CompanyId == companyId && pr.ReturnDate >= prevFromDate && pr.ReturnDate < fromDate)
            .ToListAsync(ct);
        var expensesTask = _context.Expenses
            .Where(e => e.CompanyId == companyId && e.ExpenseDate >= fromDate)
            .ToListAsync(ct);
        var prevExpensesTask = _context.Expenses
            .Where(e => e.CompanyId == companyId && e.ExpenseDate >= prevFromDate && e.ExpenseDate < fromDate)
            .ToListAsync(ct);
        var productsTask = _context.Products
            .Where(p => p.CompanyId == companyId)
            .ToListAsync(ct);
        var suppliersCountTask = _context.Suppliers.CountAsync(s => s.CompanyId == companyId, ct);
        var customersCountTask = _context.Customers.CountAsync(c => c.CompanyId == companyId, ct);
        var allSalesTask = _context.Sales
            .Where(s => s.CompanyId == companyId)
            .Include(s => s.Customer)
            .Include(s => s.Items).ThenInclude(i => i.Product)
            .OrderByDescending(s => s.SaleDate)
            .Take(10)
            .ToListAsync(ct);

        await Task.WhenAll(
            salesTask, prevSalesTask, purchasesTask, prevPurchasesTask,
            salesReturnsTask, prevSalesReturnsTask, purchaseReturnsTask, prevPurchaseReturnsTask,
            expensesTask, prevExpensesTask, productsTask,
            suppliersCountTask, customersCountTask, allSalesTask);

        var sales = salesTask.Result;
        var prevSales = prevSalesTask.Result;
        var purchases = purchasesTask.Result;
        var prevPurchases = prevPurchasesTask.Result;
        var salesReturns = salesReturnsTask.Result;
        var prevSalesReturns = prevSalesReturnsTask.Result;
        var purchaseReturns = purchaseReturnsTask.Result;
        var prevPurchaseReturns = prevPurchaseReturnsTask.Result;
        var expenses = expensesTask.Result;
        var prevExpenses = prevExpensesTask.Result;
        var products = productsTask.Result;
        var suppliersCount = suppliersCountTask.Result;
        var customersCount = customersCountTask.Result;
        var allSales = allSalesTask.Result;

        var totalSales = sales.Sum(s => s.TotalAmount);
        var prevTotalSales = prevSales.Sum(s => s.TotalAmount);
        var totalPurchase = purchases.Sum(p => p.TotalAmount);
        var prevTotalPurchase = prevPurchases.Sum(p => p.TotalAmount);
        var totalSalesReturn = salesReturns.Sum(sr => sr.TotalAmount);
        var prevSalesReturn = prevSalesReturns.Sum(sr => sr.TotalAmount);
        var totalPurchaseReturn = purchaseReturns.Sum(pr => pr.TotalAmount);
        var prevPurchaseReturn = prevPurchaseReturns.Sum(pr => pr.TotalAmount);
        var totalExpensesAmount = expenses.Sum(e => e.Amount);
        var prevExpensesAmount = prevExpenses.Sum(e => e.Amount);

        var invoiceDue = sales.Where(s => s.PaymentStatus != "PAID").Sum(s => s.TotalAmount);
        var profit = totalSales - totalPurchase - totalExpensesAmount;

        var lowStockProducts = products
            .Where(p => p.Stock <= p.ReorderLevel)
            .OrderBy(p => p.Stock)
            .Take(10)
            .Select(p => new LowStockProductDto
            {
                ProductId = p.Id,
                Name = p.Name,
                Sku = p.SKU,
                ImageUrl = p.ImageUrl ?? string.Empty,
                StockQuantity = p.Stock,
                AlertQuantity = p.ReorderLevel,
            }).ToList();

        var salesCount = sales.Count;
        var prevSalesCount = prevSales.Count;
        var firstTimeCustomers = sales.Select(s => s.CustomerId).Distinct().Count();
        var allTimeCustomers = await _context.Sales
            .Where(s => s.CompanyId == companyId && s.SaleDate < fromDate)
            .Select(s => s.CustomerId)
            .Distinct()
            .CountAsync(ct);
        var returnCustomers = allTimeCustomers > 0
            ? Math.Max(0, firstTimeCustomers - allTimeCustomers)
            : 0;
        var firstTimeInPeriod = Math.Max(1, firstTimeCustomers);
        var firstTimePercent = customersCount > 0
            ? Math.Round((decimal)firstTimeInPeriod / customersCount * 100, 1)
            : 0;
        var returnPercent = customersCount > 0
            ? Math.Round((decimal)returnCustomers / customersCount * 100, 1)
            : 0;

        var topProducts = sales
            .SelectMany(s => s.Items)
            .GroupBy(i => new { i.ProductId, Name = i.Product?.Name ?? "Unknown", ImageUrl = i.Product?.ImageUrl ?? "", i.Product?.SalePrice })
            .Select(g => new TopSellingProductDto
            {
                ProductId = g.Key.ProductId,
                Name = g.Key.Name,
                ImageUrl = g.Key.ImageUrl ?? string.Empty,
                SalePrice = g.Key.SalePrice ?? 0,
                SalesCount = g.Sum(i => i.Quantity),
                TotalRevenue = g.Sum(i => i.Quantity * i.UnitPrice),
            })
            .OrderByDescending(p => p.SalesCount)
            .Take(10)
            .ToList();

        var recentSales = allSales
            .Take(5)
            .Select(s => new RecentSaleDto
            {
                Id = s.Id,
                CustomerName = s.Customer?.Name ?? "Unknown",
                ProductName = s.Items.FirstOrDefault()?.Product?.Name ?? "",
                CategoryName = s.Items.FirstOrDefault()?.Product?.Category ?? "",
                TotalAmount = s.TotalAmount,
                Status = s.Status,
                SaleDate = s.SaleDate,
            }).ToList();

        var chartData = BuildChartData(sales, purchases, fromDate, now, request.Period);
        var totalSalesAmount = sales.Sum(s => s.TotalAmount);
        var totalPurchaseAmount = purchases.Sum(p => p.TotalAmount);
        var ordersCount = sales.Count + purchases.Count;

        return new DashboardDataDto
        {
            TotalSales = totalSales,
            SalesChange = prevTotalSales > 0 ? Math.Round((totalSales - prevTotalSales) / prevTotalSales * 100, 1) : 0,
            TotalSalesReturn = totalSalesReturn,
            SalesReturnChange = prevSalesReturn > 0 ? Math.Round((totalSalesReturn - prevSalesReturn) / prevSalesReturn * 100, 1) : 0,
            TotalPurchase = totalPurchase,
            PurchaseChange = prevTotalPurchase > 0 ? Math.Round((totalPurchase - prevTotalPurchase) / prevTotalPurchase * 100, 1) : 0,
            TotalPurchaseReturn = totalPurchaseReturn,
            PurchaseReturnChange = prevPurchaseReturn > 0 ? Math.Round((totalPurchaseReturn - prevPurchaseReturn) / prevPurchaseReturn * 100, 1) : 0,
            Profit = profit,
            ProfitLabel = "vs Last Month",
            ProfitChange = 0,
            InvoiceDue = invoiceDue,
            InvoiceDueLabel = "vs Last Month",
            InvoiceDueChange = 0,
            TotalExpenses = totalExpensesAmount,
            ExpensesLabel = "vs Last Month",
            ExpensesChange = prevExpensesAmount > 0 ? Math.Round((totalExpensesAmount - prevExpensesAmount) / prevExpensesAmount * 100, 1) : 0,
            TotalPaymentReturns = totalPurchaseReturn,
            PaymentReturnsLabel = "vs Last Month",
            PaymentReturnsChange = prevPurchaseReturn > 0 ? Math.Round((totalPurchaseReturn - prevPurchaseReturn) / prevPurchaseReturn * 100, 1) : 0,
            SalesPurchaseChart = chartData,
            TotalSalesAmount = totalSalesAmount,
            TotalPurchaseAmount = totalPurchaseAmount,
            SuppliersCount = suppliersCount,
            CustomersCount = customersCount,
            OrdersCount = ordersCount,
            FirstTimeCustomers = firstTimeCustomers,
            FirstTimeCustomersPercent = firstTimePercent,
            ReturnCustomers = returnCustomers,
            ReturnCustomersPercent = returnPercent,
            TopSellingProducts = topProducts,
            LowStockProducts = lowStockProducts,
            RecentSales = recentSales,
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
        DateTime fromDate, DateTime now, string period)
    {
        var months = new List<SalesPurchaseChartItemDto>();
        var start = new DateTime(fromDate.Year, fromDate.Month, 1);
        var end = new DateTime(now.Year, now.Month, 1);

        for (var m = start; m <= end; m = m.AddMonths(1))
        {
            var label = m.ToString("MMM yyyy");
            var saleTotal = sales
                .Where(s => s.SaleDate.Year == m.Year && s.SaleDate.Month == m.Month)
                .Sum(s => s.TotalAmount);
            var purchaseTotal = purchases
                .Where(p => p.OrderDate.Year == m.Year && p.OrderDate.Month == m.Month)
                .Sum(p => p.TotalAmount);
            months.Add(new SalesPurchaseChartItemDto
            {
                Month = label,
                Purchase = purchaseTotal,
                Sales = saleTotal,
            });
        }

        return months;
    }
}
