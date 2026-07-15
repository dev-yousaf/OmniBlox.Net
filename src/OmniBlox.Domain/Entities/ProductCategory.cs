namespace OmniBlox.Domain.Entities;

public class ProductCategory : BaseEntity, Interfaces.ITenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Enums.ActiveStatus Status { get; set; } = Enums.ActiveStatus.ACTIVE;
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public ICollection<SubCategory> SubCategories { get; set; } = [];
}
