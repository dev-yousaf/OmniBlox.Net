using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.SalesReturns.DTOs;

namespace OmniBlox.Application.Features.SalesReturns.Queries;

public record GetSalesReturnsQuery : IRequest<SalesReturnsListResponse>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Search { get; init; }
    public string? Status { get; init; }
    public Guid? WarehouseId { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
}

public class GetSalesReturnsQueryHandler : IRequestHandler<GetSalesReturnsQuery, SalesReturnsListResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetSalesReturnsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SalesReturnsListResponse> Handle(GetSalesReturnsQuery request, CancellationToken ct)
    {
        var query = _context.SalesReturns
            .Include(r => r.Warehouse)
            .Include(r => r.Sale)
            .Include(r => r.Items)
                .ThenInclude(i => i.Product)
            .AsQueryable()
            .Where(e => e.CompanyId == _currentUser.CompanyId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(r =>
                r.ReferenceNumber.ToLower().Contains(search) ||
                r.Reason!.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(r => r.Status == request.Status);

        if (request.WarehouseId.HasValue)
            query = query.Where(r => r.WarehouseId == request.WarehouseId.Value);

        if (request.DateFrom.HasValue)
            query = query.Where(r => r.ReturnDate >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(r => r.ReturnDate <= request.DateTo.Value);

        var total = await query.CountAsync(ct);
        var pages = (int)Math.Ceiling(total / (double)request.PageSize);

        var returns = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new SalesReturnsListResponse
        {
            Returns = returns.Select(SalesReturnSummaryDto.FromEntity).ToList(),
            Total = total,
            Pages = pages,
        };
    }
}
