using OmniBlox.Domain.Entities;

namespace OmniBlox.Application.Features.Units.DTOs;

public record UnitDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ShortName { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Status { get; init; } = "ACTIVE";
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }

    public static UnitDto FromEntity(Unit entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        ShortName = entity.ShortName,
        Slug = entity.Slug,
        Description = entity.Description,
        Status = entity.Status.ToString(),
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
    };
}

public record CreateUnitRequest
{
    public string Name { get; init; } = string.Empty;
    public string ShortName { get; init; } = string.Empty;
    public string? Slug { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
}

public record UpdateUnitRequest
{
    public string? Name { get; init; }
    public string? ShortName { get; init; }
    public string? Slug { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
}
