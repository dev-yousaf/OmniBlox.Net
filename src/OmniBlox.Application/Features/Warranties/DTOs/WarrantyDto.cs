using OmniBlox.Domain.Entities;

namespace OmniBlox.Application.Features.Warranties.DTOs;

public record WarrantyDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Duration { get; init; }
    public string DurationType { get; init; } = "days";
    public string? Description { get; init; }
    public string Status { get; init; } = "ACTIVE";
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }

    public static WarrantyDto FromEntity(Warranty entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Duration = entity.Duration,
        DurationType = entity.DurationType,
        Description = entity.Description,
        Status = entity.Status.ToString(),
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
    };
}

public record CreateWarrantyRequest
{
    public string Name { get; init; } = string.Empty;
    public int Duration { get; init; }
    public string? DurationType { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
}

public record UpdateWarrantyRequest
{
    public string? Name { get; init; }
    public int? Duration { get; init; }
    public string? DurationType { get; init; }
    public string? Description { get; init; }
    public string? Status { get; init; }
}
