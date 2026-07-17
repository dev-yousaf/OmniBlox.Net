using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Team.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Team.Queries;

public record GetUserQuery : IRequest<TeamUserDto>
{
    public Guid Id { get; init; }
}

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, TeamUserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetUserQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<TeamUserDto> Handle(GetUserQuery request, CancellationToken ct)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id && u.CompanyId == _currentUser.CompanyId, ct);

        if (user is null)
            throw new NotFoundException(nameof(User), request.Id);

        return TeamUserDto.FromEntity(user);
    }
}
