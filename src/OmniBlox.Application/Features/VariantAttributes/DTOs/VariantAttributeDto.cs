using System.Text.Json;
using OmniBlox.Domain.Entities;

namespace OmniBlox.Application.Features.VariantAttributes.DTOs;

public record VariantAttributeDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public JsonDocument? Values { get; init; }
    public string? Description { get; init; }
    public string Status { get; init; } = "ACTIVE";
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }

    public static VariantAttributeDto FromEntity(VariantAttribute entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Slug = entity.Slug,
        Values = entity.Values,
        Description = entity.Description,
        Status = entity.Status.ToString(),
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
    };
}

public record CreateVariantAttributeRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Slug { get; init; }
    public JsonDocument? Values { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
}

public record UpdateVariantAttributeRequest
{
    public string? Name { get; init; }
    public string? Slug { get; init; }
    public JsonDocument? Values { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
}
