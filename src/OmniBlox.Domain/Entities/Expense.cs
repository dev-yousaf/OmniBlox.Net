using OmniBlox.Domain.Interfaces;

namespace OmniBlox.Domain.Entities;

public class Expense : BaseEntity, ITenantEntity
{
    public string Reference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string? Description { get; set; }
    public string Vendor { get; set; } = string.Empty;
    public string Status { get; set; } = "PENDING";
    public string? PaymentMethod { get; set; }
    public Guid CategoryId { get; set; }
    public ExpenseCategory Category { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid? PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }
    public Guid? SaleId { get; set; }
    public Sale? Sale { get; set; }
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public ICollection<ExpenseAttachment> Attachments { get; set; } = new List<ExpenseAttachment>();
}
