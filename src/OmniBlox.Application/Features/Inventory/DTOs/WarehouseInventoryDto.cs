namespace OmniBlox.Application.Features.Inventory.DTOs;

public record WarehouseInventoryDto
{
    public Guid WarehouseId { get; init; }
    public string WarehouseName { get; init; } = string.Empty;
    public string? Location { get; init; }
    public int TotalProducts { get; init; }
    public decimal TotalStockValue { get; init; }
    public int LowStockCount { get; init; }
    public int OutOfStockCount { get; init; }
    public List<InventoryDto> Inventory { get; init; } = [];
}
