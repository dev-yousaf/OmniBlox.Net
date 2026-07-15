namespace OmniBlox.Domain.Entities;

public class StockAdjustment : BaseEntity, Interfaces.ITenantEntity
{
    public string ReferenceNumber { get; set; } = string.Empty;
    public DateTime AdjustmentDate { get; set; }
    public string? Notes { get; set; }
    public string? DocumentUrl { get; set; }
    public string Type { get; set; } = "ADDITION";
    public int TotalItems { get; set; }
    public int NetChange { get; set; }
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
    public ICollection<StockAdjustmentItem> Items { get; set; } = new List<StockAdjustmentItem>();
}
