using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Customers.DTOs;

namespace OmniBlox.Application.Features.Customers.Queries;

public record GetCustomersQuery : IRequest<CustomerListResponse>
{
    public int Page { get; init; } = 1;
    public int Limit { get; init; } = 20;
    public string? Search { get; init; }
}

public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, CustomerListResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetCustomersQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<CustomerListResponse> Handle(GetCustomersQuery request, CancellationToken ct)
    {
        var query = _context.Customers.Where(e => e.CompanyId == _currentUser.CompanyId).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(search) ||
                (c.Email != null && c.Email.ToLower().Contains(search)) ||
                (c.Phone != null && c.Phone.ToLower().Contains(search)));
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.Page - 1) * request.Limit)
            .Take(request.Limit)
            .ToListAsync(ct);

        return new CustomerListResponse
        {
            Customers = items.Select(CustomerDto.FromEntity).ToList(),
            Total = total,
            Pages = (int)Math.Ceiling((double)total / request.Limit),
        };
    }
}
