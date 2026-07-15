using OmniBlox.Domain.Entities;

namespace OmniBlox.Application.Features.SubCategories.DTOs;

public record SubCategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Code { get; init; }
    public string? ImageUrl { get; init; }
    public string? Description { get; init; }
    public string Status { get; init; } = "ACTIVE";
    public Guid CategoryId { get; init; }
    public Guid CompanyId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public SubCategoryCategoryDto? Category { get; init; }

    public static SubCategoryDto FromEntity(SubCategory entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Slug = entity.Slug,
        Code = entity.Code,
        ImageUrl = entity.ImageUrl,
        Description = entity.Description,
        Status = entity.Status.ToString(),
        CategoryId = entity.CategoryId,
        CompanyId = entity.CompanyId,
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
        Category = entity.Category is null ? null : new SubCategoryCategoryDto { Id = entity.Category.Id, Name = entity.Category.Name, Slug = entity.Category.Slug },
    };
}

public record SubCategoryCategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
}

public record CreateSubCategoryRequest
{
    public string Name { get; init; } = string.Empty;
    public Guid CategoryId { get; init; }
    public string? Slug { get; init; }
    public string? Code { get; init; }
    public string? ImageUrl { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
}

public record UpdateSubCategoryRequest
{
    public string? Name { get; init; }
    public Guid? CategoryId { get; init; }
    public string? Slug { get; init; }
    public string? Code { get; init; }
    public string? ImageUrl { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
}
