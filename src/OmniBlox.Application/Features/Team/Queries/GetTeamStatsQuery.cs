using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Team.DTOs;
using OmniBlox.Domain.Enums;

namespace OmniBlox.Application.Features.Team.Queries;

public record GetTeamStatsQuery : IRequest<TeamStatsDto>
{
}

public class GetTeamStatsQueryHandler : IRequestHandler<GetTeamStatsQuery, TeamStatsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetTeamStatsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<TeamStatsDto> Handle(GetTeamStatsQuery request, CancellationToken ct)
    {
        var companyId = _currentUser.CompanyId;

        var totalUsers = await _context.Users.CountAsync(u => u.CompanyId == companyId, ct);
        var adminCount = await _context.Users.CountAsync(u => u.CompanyId == companyId && u.Role == UserRole.ADMIN, ct);
        var managerCount = await _context.Users.CountAsync(u => u.CompanyId == companyId && u.Role == UserRole.MANAGER, ct);
        var staffCount = await _context.Users.CountAsync(u => u.CompanyId == companyId && u.Role == UserRole.OBSERVER, ct);
        var activeUsers = await _context.Users.CountAsync(u => u.CompanyId == companyId && u.Status == UserStatus.ACTIVE, ct);
        var inactiveUsers = await _context.Users.CountAsync(u => u.CompanyId == companyId && u.Status == UserStatus.SUSPENDED, ct);
        var invitedUsers = await _context.Users.CountAsync(u => u.CompanyId == companyId && u.Status == UserStatus.INVITED, ct);

        return new TeamStatsDto
        {
            TotalUsers = totalUsers,
            AdminCount = adminCount,
            ManagerCount = managerCount,
            StaffCount = staffCount,
            ActiveUsers = activeUsers,
            InactiveUsers = inactiveUsers,
            InvitedUsers = invitedUsers,
        };
    }
}
