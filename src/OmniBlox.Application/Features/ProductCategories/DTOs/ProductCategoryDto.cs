using OmniBlox.Domain.Entities;

namespace OmniBlox.Application.Features.ProductCategories.DTOs;

public record ProductCategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Status { get; init; } = "ACTIVE";
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }

    public static ProductCategoryDto FromEntity(ProductCategory entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Slug = entity.Slug,
        Description = entity.Description,
        Status = entity.Status.ToString(),
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
    };
}

public record CreateProductCategoryRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Slug { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
}

public record UpdateProductCategoryRequest
{
    public string? Name { get; init; }
    public string? Slug { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
}

public record DeleteCategoryResponse
{
    public string Message { get; init; } = string.Empty;
    public List<AffectedProduct> AffectedProducts { get; init; } = [];
}

public record AffectedProduct
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
}

public record BulkDeleteResponse
{
    public string Message { get; init; } = string.Empty;
    public List<Guid> Deleted { get; init; } = [];
    public List<FailedItem> Failed { get; init; } = [];
}

public record FailedItem
{
    public Guid Id { get; init; }
    public string Error { get; init; } = string.Empty;
}
