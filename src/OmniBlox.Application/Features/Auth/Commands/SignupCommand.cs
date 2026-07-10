using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Auth.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Auth.Commands;

public record SignupCommand : IRequest<AuthResponse>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string CompanyName { get; init; } = string.Empty;
}

public class SignupCommandHandler : IRequestHandler<SignupCommand, AuthResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public SignupCommandHandler(IApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> Handle(SignupCommand request, CancellationToken ct)
    {
        var existingUser = await _context.Users
            .AnyAsync(u => u.Email == request.Email, ct);

        if (existingUser)
            throw new ConflictException("Email is already registered.");

        var company = new Company
        {
            Name = request.CompanyName,
            Email = request.Email,
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync(ct);

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Name = request.Name,
            Role = UserRole.OWNER,
            Status = UserStatus.ACTIVE,
            CompanyId = company.Id,
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);

        var token = _jwtService.GenerateToken(user, company);

        return new AuthResponse
        {
            Token = token,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role.ToString(),
            CompanyId = company.Id,
        };
    }
}

public class SignupCommandValidator : AbstractValidator<SignupCommand>
{
    public SignupCommandValidator()
    {
        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(v => v.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");

        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(v => v.CompanyName)
            .NotEmpty().WithMessage("Company name is required.");
    }
}
