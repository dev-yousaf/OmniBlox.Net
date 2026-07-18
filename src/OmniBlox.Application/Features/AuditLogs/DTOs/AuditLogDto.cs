using OmniBlox.Domain.Entities;

namespace OmniBlox.Application.Features.AuditLogs.DTOs;

public record AuditLogDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string UserRole { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string Entity { get; init; } = string.Empty;
    public string? EntityId { get; init; }
    public string? Details { get; init; }
    public DateTime CreatedAt { get; init; }

    public static AuditLogDto FromEntity(AuditLog log) => new()
    {
        Id = log.Id,
        UserId = log.UserId,
        UserName = log.User.Name,
        UserRole = log.User.Role.ToString(),
        Action = log.Action,
        Entity = log.Entity,
        EntityId = log.EntityId,
        Details = log.Details,
        CreatedAt = log.CreatedAt,
    };
}

public record AuditLogListResponse
{
    public List<AuditLogDto> Logs { get; init; } = [];
    public int Total { get; init; }
    public int Pages { get; init; }
    public int Page { get; init; }
    public int Limit { get; init; }
}
