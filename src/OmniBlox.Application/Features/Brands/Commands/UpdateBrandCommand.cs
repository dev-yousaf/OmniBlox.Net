using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Brands.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;
using OmniBlox.Shared.Extensions;

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
    private readonly ICrudService<Brand, BrandDto> _crud;
    public UpdateBrandCommandHandler(IApplicationDbContext context, ICrudService<Brand, BrandDto> crud)
    {
        _context = context;
        _crud = crud;
    }

    public async Task<BrandDto> Handle(UpdateBrandCommand request, CancellationToken ct)
    {
        if (request.Slug is not null)
        {
            var slug = request.Slug.ToLowerInvariant();
            var exists = await _context.Brands.AnyAsync(x => x.Slug == slug && x.Id != request.Id, ct);
            if (exists) throw new ConflictException($"Brand with slug '{slug}' already exists.");
        }

        return await _crud.UpdateAsync(request.Id, entity =>
        {
            if (request.Name is not null) entity.Name = request.Name;
            if (request.ImageUrl is not null) entity.ImageUrl = request.ImageUrl;
            if (request.Description is not null) entity.Description = request.Description;
            if (request.Status is not null) entity.Status = request.Status.ToEnumOrDefault(entity.Status);
            if (request.Slug is not null) entity.Slug = request.Slug.ToLowerInvariant();
        }, BrandDto.FromEntity, ct);
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
