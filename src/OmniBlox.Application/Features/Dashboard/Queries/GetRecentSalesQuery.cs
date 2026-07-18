using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Dashboard.DTOs;

namespace OmniBlox.Application.Features.Dashboard.Queries;

public record GetRecentSalesQuery : IRequest<List<RecentSaleDto>>
{
    public string Period { get; init; } = "1Y";
}

public class GetRecentSalesQueryHandler : IRequestHandler<GetRecentSalesQuery, List<RecentSaleDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetRecentSalesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<RecentSaleDto>> Handle(GetRecentSalesQuery request, CancellationToken ct)
    {
        var companyId = _currentUser.CompanyId;
        var fromDate = request.Period switch
        {
            "1D" => DateTime.UtcNow.AddDays(-1),
            "1W" => DateTime.UtcNow.AddDays(-7),
            "1M" => DateTime.UtcNow.AddMonths(-1),
            "3M" => DateTime.UtcNow.AddMonths(-3),
            "6M" => DateTime.UtcNow.AddMonths(-6),
            _ => DateTime.UtcNow.AddYears(-1),
        };

        var sales = await _context.Sales
            .Include(s => s.Customer)
            .Include(s => s.Items).ThenInclude(i => i.Product)
            .Where(s => s.CompanyId == companyId && s.SaleDate >= fromDate)
            .OrderByDescending(s => s.SaleDate)
            .Take(10)
            .ToListAsync(ct);

        return sales.Select(s => new RecentSaleDto
        {
            Id = s.Id,
            CustomerName = s.Customer?.Name ?? "Unknown",
            ProductName = s.Items.FirstOrDefault()?.Product?.Name ?? "",
            CategoryName = s.Items.FirstOrDefault()?.Product?.Category ?? "",
            TotalAmount = s.TotalAmount,
            Status = s.Status,
            SaleDate = s.SaleDate,
        }).ToList();
    }
}
