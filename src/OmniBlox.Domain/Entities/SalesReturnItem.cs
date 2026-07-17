namespace OmniBlox.Domain.Entities;

public class SalesReturnItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public Guid SalesReturnId { get; set; }
    public SalesReturn SalesReturn { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public Guid? SaleItemId { get; set; }
    public SaleItem? SaleItem { get; set; }
}
