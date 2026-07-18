using OmniBlox.Domain.Interfaces;

namespace OmniBlox.Domain.Entities;

public class AuditLog : BaseEntity, ITenantEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? Details { get; set; }
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
}
