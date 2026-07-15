using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Auth.DTOs;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Auth.Commands;

public record UpdateProfileCommand : IRequest<UserDto>
{
    public string? Name { get; init; }
    public string? CompanyName { get; init; }
    public string? Industry { get; init; }
    public string? OtherIndustry { get; init; }
    public string? Country { get; init; }
}

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateProfileCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<UserDto> Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        var user = await _context.Users
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, ct);

        if (user is null)
            throw new NotFoundException("User", _currentUser.UserId);

        if (request.Name is not null)
            user.Name = request.Name;

        if (request.CompanyName is not null)
            user.Company.Name = request.CompanyName;

        if (request.Industry is not null)
        {
            user.Company.Industry = request.Industry;
            if (request.Industry != "other")
                user.Company.OtherIndustry = null;
        }

        if (request.OtherIndustry is not null)
            user.Company.OtherIndustry = request.OtherIndustry;

        if (request.Country is not null)
            user.Company.Country = request.Country;

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role.ToString(),
            CompanyId = user.CompanyId,
            Company = new CompanyDto
            {
                Id = user.Company.Id,
                Name = user.Company.Name,
                WorkspaceUrl = user.Company.WorkspaceUrl,
                Industry = user.Company.Industry,
                OtherIndustry = user.Company.OtherIndustry,
                Country = user.Company.Country,
            },
        };
    }
}

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(v => v.Name)
            .MaximumLength(200).When(v => v.Name is not null);

        RuleFor(v => v.CompanyName)
            .MaximumLength(200).When(v => v.CompanyName is not null);
    }
}
