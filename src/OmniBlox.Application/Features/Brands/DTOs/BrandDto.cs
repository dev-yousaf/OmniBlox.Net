using OmniBlox.Domain.Entities;

namespace OmniBlox.Application.Features.Brands.DTOs;

public record BrandDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public string? Description { get; init; }
    public string Status { get; init; } = "ACTIVE";
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }

    public static BrandDto FromEntity(Brand entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Slug = entity.Slug,
        ImageUrl = entity.ImageUrl,
        Description = entity.Description,
        Status = entity.Status.ToString(),
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
    };
}

public record CreateBrandRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Slug { get; init; }
    public string? ImageUrl { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
}

public record UpdateBrandRequest
{
    public string? Name { get; init; }
    public string? Slug { get; init; }
    public string? ImageUrl { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
}
