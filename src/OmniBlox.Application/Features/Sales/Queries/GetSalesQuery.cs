using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Sales.DTOs;

namespace OmniBlox.Application.Features.Sales.Queries;

public record GetSalesQuery : IRequest<SalesListResponse>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Search { get; init; }
    public string? Status { get; init; }
    public string? PaymentStatus { get; init; }
    public Guid? WarehouseId { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public Guid? ProductId { get; init; }
}

public class GetSalesQueryHandler : IRequestHandler<GetSalesQuery, SalesListResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetSalesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SalesListResponse> Handle(GetSalesQuery request, CancellationToken ct)
    {
        var query = _context.Sales
            .Include(s => s.Customer)
            .Include(s => s.Warehouse)
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .AsQueryable()
            .Where(e => e.CompanyId == _currentUser.CompanyId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(s =>
                s.InvoiceNumber.ToLower().Contains(search) ||
                s.Customer.Name.ToLower().Contains(search) ||
                s.Customer.Email!.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(s => s.Status == request.Status);

        if (!string.IsNullOrWhiteSpace(request.PaymentStatus))
            query = query.Where(s => s.PaymentStatus == request.PaymentStatus);

        if (request.WarehouseId.HasValue)
            query = query.Where(s => s.WarehouseId == request.WarehouseId);

        if (request.DateFrom.HasValue)
            query = query.Where(s => s.SaleDate >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(s => s.SaleDate <= request.DateTo.Value);

        if (request.ProductId.HasValue)
            query = query.Where(s => s.Items.Any(i => i.ProductId == request.ProductId.Value));

        var total = await query.CountAsync(ct);
        var pages = (int)Math.Ceiling(total / (double)request.PageSize);

        var sales = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new SalesListResponse
        {
            Sales = sales.Select(SaleSummaryDto.FromEntity).ToList(),
            Total = total,
            Pages = pages,
        };
    }
}
