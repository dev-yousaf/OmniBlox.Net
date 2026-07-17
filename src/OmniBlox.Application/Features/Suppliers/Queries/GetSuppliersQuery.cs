using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Suppliers.DTOs;

namespace OmniBlox.Application.Features.Suppliers.Queries;

public record GetSuppliersQuery : IRequest<SupplierListResponse>
{
    public int Page { get; init; } = 1;
    public int Limit { get; init; } = 20;
    public string? Search { get; init; }
}

public class GetSuppliersQueryHandler : IRequestHandler<GetSuppliersQuery, SupplierListResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetSuppliersQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SupplierListResponse> Handle(GetSuppliersQuery request, CancellationToken ct)
    {
        var query = _context.Suppliers.Where(e => e.CompanyId == _currentUser.CompanyId).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(search) ||
                (s.Email != null && s.Email.ToLower().Contains(search)) ||
                (s.Phone != null && s.Phone.ToLower().Contains(search)));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((request.Page - 1) * request.Limit)
            .Take(request.Limit)
            .ToListAsync(ct);

        return new SupplierListResponse
        {
            Suppliers = items.Select(SupplierDto.FromEntity).ToList(),
            Total = total,
            Pages = (int)Math.Ceiling((double)total / request.Limit),
        };
    }
}
