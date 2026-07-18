using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Brands.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Brands.Commands;

public record CreateBrandCommand : IRequest<BrandDto>
{
    public string Name { get; init; } = string.Empty;
    public string? Slug { get; init; }
    public string? ImageUrl { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
}

public class CreateBrandCommandHandler : IRequestHandler<CreateBrandCommand, BrandDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public CreateBrandCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<BrandDto> Handle(CreateBrandCommand request, CancellationToken ct)
    {
        var companyId = _currentUser.CompanyId;
        var slug = request.Slug?.ToLowerInvariant() ?? request.Name.ToLowerInvariant().Replace(" ", "-");

        var exists = await _context.Brands.AnyAsync(x => x.CompanyId == companyId && x.Slug == slug, ct);
        if (exists) throw new ConflictException($"Brand with slug '{slug}' already exists.");

        var entity = new Brand
        {
            Name = request.Name,
            Slug = slug,
            ImageUrl = request.ImageUrl,
            Description = request.Description,
            Status = request.Status is not null && Enum.TryParse<ActiveStatus>(request.Status, true, out var s) ? s : ActiveStatus.ACTIVE,
            CompanyId = companyId,
        };

        _context.Brands.Add(entity);
        await _context.SaveChangesAsync(ct);
        return BrandDto.FromEntity(entity);
    }
}

public class CreateBrandCommandValidator : AbstractValidator<CreateBrandCommand>
{
    public CreateBrandCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(200);
    }
}
