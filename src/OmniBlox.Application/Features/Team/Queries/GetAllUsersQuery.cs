using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Team.DTOs;

namespace OmniBlox.Application.Features.Team.Queries;

public record GetAllUsersQuery : IRequest<List<TeamUserDto>>
{
}

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<TeamUserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAllUsersQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<TeamUserDto>> Handle(GetAllUsersQuery request, CancellationToken ct)
    {
        var users = await _context.Users
            .Where(u => u.CompanyId == _currentUser.CompanyId)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(ct);

        return users.Select(u => TeamUserDto.FromEntity(u)).ToList();
    }
}
