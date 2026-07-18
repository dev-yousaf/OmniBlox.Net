using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Units.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;
using OmniBlox.Shared.Extensions;

namespace OmniBlox.Application.Features.Units.Commands;

public record CreateUnitCommand : IRequest<UnitDto>
{
    public string Name { get; init; } = string.Empty;
    public string ShortName { get; init; } = string.Empty;
    public string? Slug { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
}

public class CreateUnitCommandHandler : IRequestHandler<CreateUnitCommand, UnitDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public CreateUnitCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<UnitDto> Handle(CreateUnitCommand request, CancellationToken ct)
    {
        var companyId = _currentUser.CompanyId;
        var slug = request.Slug?.ToLowerInvariant() ?? request.Name.ToLowerInvariant().Replace(" ", "-");
        var exists = await _context.Units.AnyAsync(x => x.CompanyId == companyId && x.Slug == slug, ct);
        if (exists) throw new ConflictException($"Unit with slug '{slug}' already exists.");

        var entity = new Domain.Entities.Unit
        {
            Name = request.Name,
            ShortName = request.ShortName,
            Slug = slug,
            Description = request.Description,
            Status = request.Status.ToEnumOrDefault(ActiveStatus.ACTIVE),
            CompanyId = companyId,
        };

        _context.Units.Add(entity);
        await _context.SaveChangesAsync(ct);
        return UnitDto.FromEntity(entity);
    }
}

public class CreateUnitCommandValidator : AbstractValidator<CreateUnitCommand>
{
    public CreateUnitCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(100);
        RuleFor(v => v.ShortName).NotEmpty().MaximumLength(20);
    }
}
