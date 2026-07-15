using System.Text.Json;

namespace OmniBlox.Domain.Entities;

public class VariantAttribute : BaseEntity, Interfaces.ITenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public JsonDocument? Values { get; set; }
    public string? Description { get; set; }
    public Enums.ActiveStatus Status { get; set; } = Enums.ActiveStatus.ACTIVE;
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
}
