using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Products.DTOs;

namespace OmniBlox.Application.Features.Products.Queries;

public record GetProductsQuery : IRequest<ProductListResponse>
{
    public int Page { get; init; } = 1;
    public int Limit { get; init; } = 20;
    public string? Search { get; init; }
    public string? Category { get; init; }
    public string? Status { get; init; }
}

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, ProductListResponse>
{
    private readonly IApplicationDbContext _context;

    public GetProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductListResponse> Handle(GetProductsQuery request, CancellationToken ct)
    {
        var query = _context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(search) ||
                p.SKU.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
            query = query.Where(p => p.Category == request.Category);

        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<Domain.Enums.ProductStatus>(request.Status, true, out var status))
            query = query.Where(p => p.Status == status);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.Limit)
            .Take(request.Limit)
            .ToListAsync(ct);

        return new ProductListResponse
        {
            Items = items.Select(ProductDto.FromEntity).ToList(),
            Total = total,
            Page = request.Page,
            Limit = request.Limit,
        };
    }
}
