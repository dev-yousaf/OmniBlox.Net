using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Products.DTOs;

namespace OmniBlox.Application.Features.Products.Queries;

public record GetExpiredProductsQuery : IRequest<ProductListResponse>
{
    public int Page { get; init; } = 1;
    public int Limit { get; init; } = 20;
}

public class GetExpiredProductsQueryHandler : IRequestHandler<GetExpiredProductsQuery, ProductListResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetExpiredProductsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ProductListResponse> Handle(GetExpiredProductsQuery request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var query = _context.Products.Where(e => e.CompanyId == _currentUser.CompanyId).Where(p => p.ExpiryDate != null && p.ExpiryDate <= now);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.ExpiryDate)
            .Skip((request.Page - 1) * request.Limit)
            .Take(request.Limit)
            .ToListAsync(ct);

        return new ProductListResponse
        {
            Products = items.Select(p => ProductDto.FromEntity(p)).ToList(),
            Total = total,
            Pages = (int)Math.Ceiling((double)total / request.Limit),
            Page = request.Page,
            Limit = request.Limit,
        };
    }
}
