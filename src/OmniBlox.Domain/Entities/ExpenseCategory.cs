using OmniBlox.Domain.Interfaces;

namespace OmniBlox.Domain.Entities;

public class ExpenseCategory : BaseEntity, ITenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
