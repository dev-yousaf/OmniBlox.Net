using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Team.Commands;

public record ChangePasswordCommand : IRequest
{
    public Guid Id { get; init; }
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ChangePasswordCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(ChangePasswordCommand request, CancellationToken ct)
    {
        var callerRole = Enum.Parse<UserRole>(_currentUser.Role);
        if (callerRole != UserRole.OWNER && callerRole != UserRole.ADMIN)
            throw new UnauthorizedException("Only owners and admins can change passwords.");

        var caller = await _context.Users
            .AsTracking().FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, ct);

        if (caller is null)
            throw new UnauthorizedException("Caller not found.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, caller.PasswordHash))
            throw new UnauthorizedException("Current password is incorrect.");

        var target = await _context.Users
            .AsTracking().FirstOrDefaultAsync(u => u.Id == request.Id && u.CompanyId == _currentUser.CompanyId, ct);

        if (target is null)
            throw new NotFoundException(nameof(User), request.Id);

        if (callerRole != UserRole.OWNER && target.Role == UserRole.ADMIN)
            throw new UnauthorizedException("Only the owner can change an admin's password.");

        target.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        target.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
    }
}

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.CurrentPassword).NotEmpty();
        RuleFor(v => v.NewPassword).NotEmpty().MinimumLength(6);
    }
}
