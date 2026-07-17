using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Quotations.DTOs;

namespace OmniBlox.Application.Features.Quotations.Queries;

public record GetQuotationsQuery : IRequest<QuotationListResponse>
{
    public int Page { get; init; } = 1;
    public int Limit { get; init; } = 20;
    public string? Status { get; init; }
    public Guid? CustomerId { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
}

public class GetQuotationsQueryHandler : IRequestHandler<GetQuotationsQuery, QuotationListResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetQuotationsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<QuotationListResponse> Handle(GetQuotationsQuery request, CancellationToken ct)
    {
        var query = _context.Quotations
            .Include(q => q.Customer)
            .AsQueryable()
            .Where(e => e.CompanyId == _currentUser.CompanyId);

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(q => q.Status == request.Status);

        if (request.CustomerId.HasValue)
            query = query.Where(q => q.CustomerId == request.CustomerId.Value);

        if (request.DateFrom.HasValue)
            query = query.Where(q => q.QuoteDate >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(q => q.QuoteDate <= request.DateTo.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(q => q.CreatedAt)
            .Skip((request.Page - 1) * request.Limit)
            .Take(request.Limit)
            .ToListAsync(ct);

        return new QuotationListResponse
        {
            Quotations = items.Select(QuotationSummaryDto.FromEntity).ToList(),
            Total = total,
            Pages = (int)Math.Ceiling((double)total / request.Limit),
        };
    }
}
