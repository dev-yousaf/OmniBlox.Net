using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.ProductCategories.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.ProductCategories.Commands;

public record CreateProductCategoryCommand : IRequest<ProductCategoryDto>
{
    public string Name { get; init; } = string.Empty;
    public string? Slug { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
}

public class CreateProductCategoryCommandHandler : IRequestHandler<CreateProductCategoryCommand, ProductCategoryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public CreateProductCategoryCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ProductCategoryDto> Handle(CreateProductCategoryCommand request, CancellationToken ct)
    {
        var companyId = _currentUser.CompanyId;
        var slug = request.Slug?.ToLowerInvariant() ?? request.Name.ToLowerInvariant().Replace(" ", "-");

        var exists = await _context.ProductCategories.AnyAsync(x => x.CompanyId == companyId && x.Slug == slug, ct);
        if (exists) throw new ConflictException($"Category with slug '{slug}' already exists.");

        var entity = new ProductCategory
        {
            Name = request.Name,
            Slug = slug,
            Description = request.Description,
            Status = request.Status is not null && Enum.TryParse<ActiveStatus>(request.Status, true, out var s) ? s : ActiveStatus.ACTIVE,
            CompanyId = companyId,
        };

        _context.ProductCategories.Add(entity);
        await _context.SaveChangesAsync(ct);
        return ProductCategoryDto.FromEntity(entity);
    }
}

public class CreateProductCategoryCommandValidator : AbstractValidator<CreateProductCategoryCommand>
{
    public CreateProductCategoryCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(200);
    }
}
