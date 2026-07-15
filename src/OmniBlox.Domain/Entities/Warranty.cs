namespace OmniBlox.Domain.Entities;

public class Warranty : BaseEntity, Interfaces.ITenantEntity
{
    public string Name { get; set; } = string.Empty;
    public int Duration { get; set; }
    public string DurationType { get; set; } = "days";
    public string? Description { get; set; }
    public Enums.ActiveStatus Status { get; set; } = Enums.ActiveStatus.ACTIVE;
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
}
