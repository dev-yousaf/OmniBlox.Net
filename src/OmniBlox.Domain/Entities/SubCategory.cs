namespace OmniBlox.Domain.Entities;

public class SubCategory : BaseEntity, Interfaces.ITenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public Enums.ActiveStatus Status { get; set; } = Enums.ActiveStatus.ACTIVE;
    public Guid CategoryId { get; set; }
    public ProductCategory Category { get; set; } = null!;
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
}
