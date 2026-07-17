using System.Security.Cryptography;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Team.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Team.Commands;

public record CreateUserCommand : IRequest<TeamUserDto>
{
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Role { get; init; }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, TeamUserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateUserCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<TeamUserDto> Handle(CreateUserCommand request, CancellationToken ct)
    {
        var callerRole = Enum.Parse<UserRole>(_currentUser.Role);
        if (callerRole != UserRole.OWNER && callerRole != UserRole.ADMIN)
            throw new UnauthorizedException("Only owners and admins can create users.");

        var targetRole = request.Role is not null
            ? Enum.Parse<UserRole>(request.Role)
            : UserRole.OBSERVER;

        if (targetRole == UserRole.ADMIN && callerRole != UserRole.OWNER)
            throw new UnauthorizedException("Only the owner can create admin users.");

        if (targetRole == UserRole.OWNER)
            throw new UnauthorizedException("Cannot create another owner.");

        var existing = await _context.Users.AnyAsync(u => u.Email == request.Email, ct);
        if (existing)
            throw new ConflictException("A user with this email already exists.");

        var placeholderHash = BCrypt.Net.BCrypt.HashPassword(Convert.ToHexString(RandomNumberGenerator.GetBytes(32)));

        var user = new User
        {
            Email = request.Email,
            PasswordHash = placeholderHash,
            Name = request.Name,
            Role = targetRole,
            Status = UserStatus.INVITED,
            CompanyId = _currentUser.CompanyId,
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var invitation = new Invitation
        {
            Token = token,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(2),
            IsUsed = false,
        };

        _context.Invitations.Add(invitation);
        await _context.SaveChangesAsync(ct);

        return TeamUserDto.FromEntity(user, token);
    }
}

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(256);

        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200);

        RuleFor(v => v.Role)
            .Must(r => r is null || r is "ADMIN" or "MANAGER" or "OBSERVER")
            .WithMessage("Role must be ADMIN, MANAGER, or OBSERVER.");
    }
}
