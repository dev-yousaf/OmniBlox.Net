using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.SubCategories.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.SubCategories.Commands;

public record CreateSubCategoryCommand : IRequest<SubCategoryDto>
{
    public string Name { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
    public string? Slug { get; init; }
    public string? Code { get; init; }
    public string? ImageUrl { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
}

public class CreateSubCategoryCommandHandler : IRequestHandler<CreateSubCategoryCommand, SubCategoryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public CreateSubCategoryCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SubCategoryDto> Handle(CreateSubCategoryCommand request, CancellationToken ct)
    {
        var companyId = _currentUser.CompanyId;
        var categoryExists = await _context.ProductCategories.AnyAsync(x => x.Id == request.CategoryId, ct);
        if (!categoryExists) throw new NotFoundException(nameof(ProductCategory), request.CategoryId);

        var slug = request.Slug?.ToLowerInvariant() ?? request.Name.ToLowerInvariant().Replace(" ", "-");

        var exists = await _context.SubCategories.AnyAsync(x => x.CompanyId == companyId && x.Slug == slug, ct);
        if (exists) throw new ConflictException($"SubCategory with slug '{slug}' already exists.");

        var entity = new SubCategory
        {
            Name = request.Name,
            CategoryId = request.CategoryId,
            Slug = slug,
            Code = request.Code,
            ImageUrl = request.ImageUrl,
            Description = request.Description,
            Status = request.Status is not null && Enum.TryParse<ActiveStatus>(request.Status, true, out var s) ? s : ActiveStatus.ACTIVE,
            CompanyId = companyId,
        };

        _context.SubCategories.Add(entity);
        await _context.SaveChangesAsync(ct);
        return SubCategoryDto.FromEntity(entity);
    }
}

public class CreateSubCategoryCommandValidator : AbstractValidator<CreateSubCategoryCommand>
{
    public CreateSubCategoryCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(200);
        RuleFor(v => v.CategoryId).NotEmpty();
    }
}
