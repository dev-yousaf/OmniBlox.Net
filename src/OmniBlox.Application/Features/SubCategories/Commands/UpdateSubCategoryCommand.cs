using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.SubCategories.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;
using OmniBlox.Shared.Extensions;

namespace OmniBlox.Application.Features.SubCategories.Commands;

public record UpdateSubCategoryCommand : IRequest<SubCategoryDto>
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public Guid? CategoryId { get; init; }
    public string? Slug { get; init; }
    public string? Code { get; init; }
    public string? ImageUrl { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
}

public class UpdateSubCategoryCommandHandler : IRequestHandler<UpdateSubCategoryCommand, SubCategoryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICrudService<SubCategory, SubCategoryDto> _crud;
    public UpdateSubCategoryCommandHandler(IApplicationDbContext context, ICrudService<SubCategory, SubCategoryDto> crud)
    {
        _context = context;
        _crud = crud;
    }

    public async Task<SubCategoryDto> Handle(UpdateSubCategoryCommand request, CancellationToken ct)
    {
        if (request.CategoryId.HasValue)
        {
            var categoryExists = await _context.ProductCategories.AnyAsync(x => x.Id == request.CategoryId.Value, ct);
            if (!categoryExists) throw new NotFoundException(nameof(ProductCategory), request.CategoryId.Value);
        }

        if (request.Slug is not null)
        {
            var slug = request.Slug.ToLowerInvariant();
            var exists = await _context.SubCategories.AnyAsync(x => x.Slug == slug && x.Id != request.Id, ct);
            if (exists) throw new ConflictException($"SubCategory with slug '{slug}' already exists.");
        }

        return await _crud.UpdateAsync(request.Id, entity =>
        {
            if (request.Name is not null) entity.Name = request.Name;
            if (request.Description is not null) entity.Description = request.Description;
            if (request.Code is not null) entity.Code = request.Code;
            if (request.ImageUrl is not null) entity.ImageUrl = request.ImageUrl;
            if (request.Status is not null) entity.Status = request.Status.ToEnumOrDefault(entity.Status);
            if (request.CategoryId.HasValue) entity.CategoryId = request.CategoryId.Value;
            if (request.Slug is not null) entity.Slug = request.Slug.ToLowerInvariant();
        }, SubCategoryDto.FromEntity, ct);
    }
}

public class UpdateSubCategoryCommandValidator : AbstractValidator<UpdateSubCategoryCommand>
{
    public UpdateSubCategoryCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.Name).MaximumLength(200).When(v => v.Name is not null);
    }
}
