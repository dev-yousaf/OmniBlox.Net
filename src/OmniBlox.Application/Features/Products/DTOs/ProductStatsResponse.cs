namespace OmniBlox.Application.Features.Products.DTOs;

public record ProductStatsResponse
{
    public int TotalProducts { get; init; }
    public int LowStockCount { get; init; }
    public decimal TotalValue { get; init; }
    public int CategoriesCount { get; init; }
}
