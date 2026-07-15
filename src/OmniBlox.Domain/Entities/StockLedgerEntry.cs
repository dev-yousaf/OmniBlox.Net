namespace OmniBlox.Domain.Entities;

public class StockLedgerEntry : BaseEntity, Interfaces.ITenantEntity
{
    public int Quantity { get; set; }
    public int Balance { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public string? Note { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public Guid? WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
}
