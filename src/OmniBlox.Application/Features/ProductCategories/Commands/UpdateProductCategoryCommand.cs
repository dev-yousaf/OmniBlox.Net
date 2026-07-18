using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.ProductCategories.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;
using OmniBlox.Shared.Extensions;

namespace OmniBlox.Application.Features.ProductCategories.Commands;

public record UpdateProductCategoryCommand : IRequest<ProductCategoryDto>
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? Slug { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
}

public class UpdateProductCategoryCommandHandler : IRequestHandler<UpdateProductCategoryCommand, ProductCategoryDto>
{
    private readonly IApplicationDbContext _context;
    public UpdateProductCategoryCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<ProductCategoryDto> Handle(UpdateProductCategoryCommand request, CancellationToken ct)
    {
        var entity = await _context.ProductCategories.AsTracking().FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null) throw new NotFoundException(nameof(ProductCategory), request.Id);

        if (request.Name is not null) entity.Name = request.Name;
        if (request.Description is not null) entity.Description = request.Description;
        if (request.Status is not null) entity.Status = request.Status.ToEnumOrDefault(entity.Status);

        if (request.Slug is not null)
        {
            var slug = request.Slug.ToLowerInvariant();
            var exists = await _context.ProductCategories.AnyAsync(x => x.Slug == slug && x.Id != request.Id, ct);
            if (exists) throw new ConflictException($"Category with slug '{slug}' already exists.");
            entity.Slug = slug;
        }

        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return ProductCategoryDto.FromEntity(entity);
    }
}

public class UpdateProductCategoryCommandValidator : AbstractValidator<UpdateProductCategoryCommand>
{
    public UpdateProductCategoryCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.Name).MaximumLength(200).When(v => v.Name is not null);
    }
}
