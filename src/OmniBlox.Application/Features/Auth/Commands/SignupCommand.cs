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
    public string WorkspaceUrl { get; init; } = string.Empty;
    public string? Industry { get; init; }
    public string? OtherIndustry { get; init; }
    public string? Country { get; init; }
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

        var existingWorkspace = await _context.Companies
            .AnyAsync(c => c.WorkspaceUrl == request.WorkspaceUrl, ct);

        if (existingWorkspace)
            throw new ConflictException("Workspace URL is already taken.");

        var company = new Company
        {
            Name = request.CompanyName,
            Email = request.Email,
            WorkspaceUrl = request.WorkspaceUrl,
            Industry = request.Industry,
            OtherIndustry = request.Industry == "other" ? request.OtherIndustry : null,
            Country = request.Country,
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

        var companyDto = new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            WorkspaceUrl = company.WorkspaceUrl,
            Industry = company.Industry,
            OtherIndustry = company.OtherIndustry,
            Country = company.Country,
        };

        return new AuthResponse
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Role = user.Role.ToString(),
                CompanyId = user.CompanyId,
                Company = companyDto,
            },
            Company = companyDto,
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
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(v => v.CompanyName)
            .NotEmpty().WithMessage("Company name is required.");

        RuleFor(v => v.WorkspaceUrl)
            .NotEmpty().WithMessage("Workspace URL is required.")
            .Matches("^[a-z0-9-]+$").WithMessage("Workspace URL must be lowercase alphanumeric with dashes.");
    }
}
