using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Team.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Team.Commands;

public record UpdateUserCommand : IRequest<TeamUserDto>
{
    public Guid Id { get; init; }
    public string? Email { get; init; }
    public string? Name { get; init; }
    public string? Role { get; init; }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, TeamUserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateUserCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<TeamUserDto> Handle(UpdateUserCommand request, CancellationToken ct)
    {
        var callerRole = Enum.Parse<UserRole>(_currentUser.Role);
        if (callerRole != UserRole.OWNER && callerRole != UserRole.ADMIN)
            throw new UnauthorizedException("Only owners and admins can update users.");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.Id && u.CompanyId == _currentUser.CompanyId, ct);

        if (user is null)
            throw new NotFoundException(nameof(User), request.Id);

        if (request.Role is not null)
        {
            var newRole = Enum.Parse<UserRole>(request.Role);
            if (newRole == UserRole.ADMIN && callerRole != UserRole.OWNER)
                throw new UnauthorizedException("Only the owner can assign the admin role.");

            if (newRole == UserRole.OWNER)
                throw new UnauthorizedException("Cannot reassign the owner role.");

            if (user.Role == UserRole.OWNER)
                throw new UnauthorizedException("Cannot change the owner's role.");

            user.Role = newRole;
        }

        if (request.Email is not null)
        {
            var duplicate = await _context.Users
                .AnyAsync(u => u.Email == request.Email && u.Id != request.Id, ct);
            if (duplicate)
                throw new ConflictException("Email is already in use.");

            user.Email = request.Email;
        }

        if (request.Name is not null)
            user.Name = request.Name;

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return TeamUserDto.FromEntity(user);
    }
}

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.Email).EmailAddress().When(v => v.Email is not null).MaximumLength(256);
        RuleFor(v => v.Name).MaximumLength(200).When(v => v.Name is not null);
        RuleFor(v => v.Role)
            .Must(r => r is null or "ADMIN" or "MANAGER" or "OBSERVER")
            .When(v => v.Role is not null);
    }
}
