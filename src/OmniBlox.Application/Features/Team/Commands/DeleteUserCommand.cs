using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Team.Commands;

public record DeleteUserCommand : IRequest
{
    public Guid Id { get; init; }
}

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteUserCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken ct)
    {
        var callerRole = Enum.Parse<UserRole>(_currentUser.Role);
        if (callerRole != UserRole.OWNER && callerRole != UserRole.ADMIN)
            throw new UnauthorizedException("Only owners and admins can delete users.");

        if (request.Id == _currentUser.UserId)
            throw new ConflictException("You cannot delete yourself.");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id && u.CompanyId == _currentUser.CompanyId, ct);

        if (user is null)
            throw new NotFoundException(nameof(User), request.Id);

        if (user.Role == UserRole.OWNER)
            throw new ConflictException("Cannot delete the owner.");

        if (callerRole != UserRole.OWNER && user.Role == UserRole.ADMIN)
            throw new UnauthorizedException("Only the owner can delete admin users.");

        var invitations = await _context.Invitations
            .Where(i => i.UserId == request.Id)
            .ToListAsync(ct);

        _context.Invitations.RemoveRange(invitations);
        _context.Users.Remove(user);
        await _context.SaveChangesAsync(ct);
    }
}
