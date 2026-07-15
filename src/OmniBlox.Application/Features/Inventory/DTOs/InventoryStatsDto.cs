namespace OmniBlox.Application.Features.Inventory.DTOs;

public record InventoryStatsDto
{
    public int TotalProducts { get; init; }
    public int TotalWarehouses { get; init; }
    public int LowStockProducts { get; init; }
    public int OutOfStockProducts { get; init; }
    public decimal TotalStockValue { get; init; }
    public int RecentAdjustments { get; init; }
}
