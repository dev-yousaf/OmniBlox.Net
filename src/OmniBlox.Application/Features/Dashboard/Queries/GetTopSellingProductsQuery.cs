using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Dashboard.DTOs;

namespace OmniBlox.Application.Features.Dashboard.Queries;

public record GetTopSellingProductsQuery : IRequest<List<TopSellingProductDto>>
{
    public string Period { get; init; } = "1Y";
}

public class GetTopSellingProductsQueryHandler : IRequestHandler<GetTopSellingProductsQuery, List<TopSellingProductDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetTopSellingProductsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<TopSellingProductDto>> Handle(GetTopSellingProductsQuery request, CancellationToken ct)
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

        var items = await _context.SaleItems
            .Include(si => si.Sale)
            .Include(si => si.Product)
            .Where(si => si.Sale!.CompanyId == companyId && si.Sale.SaleDate >= fromDate)
            .ToListAsync(ct);

        return items
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
    }
}
