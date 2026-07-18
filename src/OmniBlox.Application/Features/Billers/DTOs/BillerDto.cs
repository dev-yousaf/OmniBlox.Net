using OmniBlox.Domain.Entities;

namespace OmniBlox.Application.Features.Billers.DTOs;

public record BillerDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public string Status { get; init; } = "ACTIVE";
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }

    public static BillerDto FromEntity(Biller entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Email = entity.Email,
        Phone = entity.Phone,
        Address = entity.Address,
        Status = entity.Status.ToString(),
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
    };
}

public record CreateBillerRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public string? Status { get; init; }
}

public record UpdateBillerRequest
{
    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public string? Status { get; init; }
}
