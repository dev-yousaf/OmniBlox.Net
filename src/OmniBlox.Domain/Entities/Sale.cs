using OmniBlox.Domain.Interfaces;

namespace OmniBlox.Domain.Entities;

public class Sale : BaseEntity, ITenantEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "DRAFT";
    public string PaymentStatus { get; set; } = "PENDING";
    public string? PaymentMethod { get; set; }
    public DateTime SaleDate { get; set; }
    public DateTime DueDate { get; set; }
    public string? ShippingAddress { get; set; }
    public string? Notes { get; set; }
    public Guid? SourceQuotationId { get; set; }
    public bool HasReturns { get; set; }

    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid? WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public Guid? BillerId { get; set; }
    public Biller? Biller { get; set; }
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
}
