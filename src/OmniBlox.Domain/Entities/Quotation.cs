using OmniBlox.Domain.Interfaces;

namespace OmniBlox.Domain.Entities;

public class Quotation : BaseEntity, ITenantEntity
{
    public string ReferenceNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "DRAFT";
    public DateTime QuoteDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }

    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public ICollection<QuotationItem> Items { get; set; } = new List<QuotationItem>();
}
