using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Brands.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Brands.Commands;

public record UpdateBrandCommand : IRequest<BrandDto>
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? Slug { get; init; }
    public string? ImageUrl { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
}

public class UpdateBrandCommandHandler : IRequestHandler<UpdateBrandCommand, BrandDto>
{
    private readonly IApplicationDbContext _context;
    public UpdateBrandCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<BrandDto> Handle(UpdateBrandCommand request, CancellationToken ct)
    {
        var entity = await _context.Brands.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null) throw new NotFoundException(nameof(Brand), request.Id);

        if (request.Name is not null) entity.Name = request.Name;
        if (request.ImageUrl is not null) entity.ImageUrl = request.ImageUrl;
        if (request.Description is not null) entity.Description = request.Description;
        if (request.Status is not null && Enum.TryParse<ActiveStatus>(request.Status, true, out var s)) entity.Status = s;

        if (request.Slug is not null)
        {
            var slug = request.Slug.ToLowerInvariant();
            var exists = await _context.Brands.AnyAsync(x => x.Slug == slug && x.Id != request.Id, ct);
            if (exists) throw new ConflictException($"Brand with slug '{slug}' already exists.");
            entity.Slug = slug;
        }

        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return BrandDto.FromEntity(entity);
    }
}

public class UpdateBrandCommandValidator : AbstractValidator<UpdateBrandCommand>
{
    public UpdateBrandCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.Name).MaximumLength(200).When(v => v.Name is not null);
    }
}
