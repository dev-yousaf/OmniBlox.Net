using OmniBlox.Domain.Interfaces;

namespace OmniBlox.Domain.Entities;

public class SalesReturn : BaseEntity, ITenantEntity
{
    public string ReferenceNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = "PENDING";
    public DateTime ReturnDate { get; set; }

    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public Guid? SaleId { get; set; }
    public Sale? Sale { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    public ICollection<SalesReturnItem> Items { get; set; } = new List<SalesReturnItem>();
}
