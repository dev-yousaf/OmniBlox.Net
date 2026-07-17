using OmniBlox.Domain.Interfaces;

namespace OmniBlox.Domain.Entities;

public class PurchaseOrder : BaseEntity, ITenantEntity
{
    public string ReferenceNumber { get; set; } = string.Empty;
    public string? BillNumber { get; set; }
    public DateTime? BillDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string PaymentStatus { get; set; } = "PENDING";
    public string? PaymentMethod { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "PENDING";
    public DateTime OrderDate { get; set; }
    public bool HasReturns { get; set; }
    public string? Notes { get; set; }

    public Guid SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid? WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
}
