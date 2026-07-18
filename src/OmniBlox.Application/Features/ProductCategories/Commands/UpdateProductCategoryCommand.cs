using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.ProductCategories.DTOs;
using OmniBlox.Domain.Entities;
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
    private readonly ICrudService<ProductCategory, ProductCategoryDto> _crud;
    public UpdateProductCategoryCommandHandler(IApplicationDbContext context, ICrudService<ProductCategory, ProductCategoryDto> crud)
    {
        _context = context;
        _crud = crud;
    }

    public async Task<ProductCategoryDto> Handle(UpdateProductCategoryCommand request, CancellationToken ct)
    {
        var slug = request.Slug?.ToLowerInvariant();
        if (slug is not null)
        {
            var exists = await _context.ProductCategories.AnyAsync(x => x.Slug == slug && x.Id != request.Id, ct);
            if (exists) throw new ConflictException($"Category with slug '{slug}' already exists.");
        }

        return await _crud.UpdateAsync(request.Id, entity =>
        {
            if (request.Name is not null) entity.Name = request.Name;
            if (request.Description is not null) entity.Description = request.Description;
            if (request.Status is not null) entity.Status = request.Status.ToEnumOrDefault(entity.Status);
            if (slug is not null) entity.Slug = slug;
        }, ProductCategoryDto.FromEntity, ct);
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
