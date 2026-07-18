namespace OmniBlox.Application.Features.Dashboard.DTOs;

public record DashboardDataDto
{
    public decimal TotalSales { get; init; }
    public decimal SalesChange { get; init; }
    public decimal TotalSalesReturn { get; init; }
    public decimal SalesReturnChange { get; init; }
    public decimal TotalPurchase { get; init; }
    public decimal PurchaseChange { get; init; }
    public decimal TotalPurchaseReturn { get; init; }
    public decimal PurchaseReturnChange { get; init; }
    public decimal Profit { get; init; }
    public string ProfitLabel { get; init; } = string.Empty;
    public decimal ProfitChange { get; init; }
    public decimal InvoiceDue { get; init; }
    public string InvoiceDueLabel { get; init; } = string.Empty;
    public decimal InvoiceDueChange { get; init; }
    public decimal TotalExpenses { get; init; }
    public string ExpensesLabel { get; init; } = string.Empty;
    public decimal ExpensesChange { get; init; }
    public decimal TotalPaymentReturns { get; init; }
    public string PaymentReturnsLabel { get; init; } = string.Empty;
    public decimal PaymentReturnsChange { get; init; }
    public List<SalesPurchaseChartItemDto> SalesPurchaseChart { get; init; } = [];
    public decimal TotalSalesAmount { get; init; }
    public decimal TotalPurchaseAmount { get; init; }
    public int SuppliersCount { get; init; }
    public int CustomersCount { get; init; }
    public int OrdersCount { get; init; }
    public int FirstTimeCustomers { get; init; }
    public decimal FirstTimeCustomersPercent { get; init; }
    public int ReturnCustomers { get; init; }
    public decimal ReturnCustomersPercent { get; init; }
    public List<TopSellingProductDto> TopSellingProducts { get; init; } = [];
    public List<LowStockProductDto> LowStockProducts { get; init; } = [];
    public List<RecentSaleDto> RecentSales { get; init; } = [];
}

public record SalesPurchaseChartItemDto
{
    public string Month { get; init; } = string.Empty;
    public decimal Purchase { get; init; }
    public decimal Sales { get; init; }
}

public record TopSellingProductDto
{
    public Guid ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public decimal SalePrice { get; init; }
    public int SalesCount { get; init; }
    public decimal TotalRevenue { get; init; }
}

public record RecentSaleDto
{
    public Guid Id { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public string CategoryName { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime SaleDate { get; init; }
}

public record LowStockProductDto
{
    public Guid ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public int StockQuantity { get; init; }
    public int AlertQuantity { get; init; }
}

public record SalesStatsResponseDto
{
    public List<SalesPurchaseChartItemDto> Chart { get; init; } = [];
    public decimal TotalRevenue { get; init; }
    public decimal TotalExpenses { get; init; }
    public decimal RevenueChange { get; init; }
    public decimal ExpenseChange { get; init; }
}
