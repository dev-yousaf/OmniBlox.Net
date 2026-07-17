using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Team.DTOs;

namespace OmniBlox.Application.Features.Team.Queries;

public record GetUsersQuery : IRequest<TeamListResponse>
{
    public int Page { get; init; } = 1;
    public int Limit { get; init; } = 20;
    public string? Search { get; init; }
    public string? Role { get; init; }
}

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, TeamListResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetUsersQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<TeamListResponse> Handle(GetUsersQuery request, CancellationToken ct)
    {
        var query = _context.Users.Where(u => u.CompanyId == _currentUser.CompanyId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(u =>
                u.Name.ToLower().Contains(search) ||
                u.Email.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.Role) &&
            Enum.TryParse<Domain.Enums.UserRole>(request.Role, true, out var role))
        {
            query = query.Where(u => u.Role == role);
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((request.Page - 1) * request.Limit)
            .Take(request.Limit)
            .ToListAsync(ct);

        return new TeamListResponse
        {
            Users = items.Select(u => TeamUserDto.FromEntity(u)).ToList(),
            Total = total,
            Pages = (int)Math.Ceiling((double)total / request.Limit),
        };
    }
}
