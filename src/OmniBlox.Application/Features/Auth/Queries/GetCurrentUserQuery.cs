using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Auth.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Auth.Queries;

public record GetCurrentUserQuery : IRequest<AuthResponse>;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetCurrentUserQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<AuthResponse> Handle(GetCurrentUserQuery request, CancellationToken ct)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, ct);

        if (user is null)
            throw new NotFoundException(nameof(User), _currentUser.UserId);

        return new AuthResponse
        {
            Email = user.Email,
            Name = user.Name,
            Role = user.Role.ToString(),
            CompanyId = user.CompanyId,
        };
    }
}
