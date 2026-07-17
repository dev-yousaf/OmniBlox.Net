using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Auth.Commands;

public record AcceptInvitationCommand : IRequest
{
    public string Token { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand>
{
    private readonly IApplicationDbContext _context;

    public AcceptInvitationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(AcceptInvitationCommand request, CancellationToken ct)
    {
        var invitation = await _context.Invitations
            .Include(i => i.User)
            .FirstOrDefaultAsync(i => i.Token == request.Token, ct);

        if (invitation is null)
            throw new NotFoundException(nameof(Invitation), request.Token);

        if (invitation.IsUsed)
            throw new ConflictException("This invitation has already been used.");

        if (invitation.ExpiresAt < DateTime.UtcNow)
            throw new ConflictException("This invitation has expired.");

        invitation.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        invitation.User.Status = UserStatus.ACTIVE;
        invitation.User.UpdatedAt = DateTime.UtcNow;

        invitation.IsUsed = true;
        await _context.SaveChangesAsync(ct);
    }
}

public class AcceptInvitationCommandValidator : AbstractValidator<AcceptInvitationCommand>
{
    public AcceptInvitationCommandValidator()
    {
        RuleFor(v => v.Token).NotEmpty();
        RuleFor(v => v.Password).NotEmpty().MinimumLength(6);
    }
}
