namespace OmniBlox.Domain.Entities;

public class StockAdjustmentItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid StockAdjustmentId { get; set; }
    public StockAdjustment StockAdjustment { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public int PreviousQuantity { get; set; }
    public int NewQuantity { get; set; }
    public int Difference { get; set; }
}
