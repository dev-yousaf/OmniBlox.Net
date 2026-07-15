using System.Text.Json.Serialization;
using OmniBlox.Domain.Entities;

namespace OmniBlox.Application.Features.Warehouses.DTOs;

public record WarehouseDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Location { get; init; }
    public string Status { get; init; } = "ACTIVE";
    [JsonPropertyName("_count")]
    public WarehouseCount Count { get; init; } = new();
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public List<WarehouseInventoryItemDto>? Inventory { get; init; }

    public static WarehouseDto FromEntity(Warehouse entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Location = entity.Location,
        Status = entity.Status.ToString(),
        CreatedAt = entity.CreatedAt,
        UpdatedAt = entity.UpdatedAt,
    };
}

public record WarehouseCount
{
    [JsonPropertyName("inventory")]
    public int ProductCount { get; init; }
}

public record WarehouseInventoryItemDto
{
    public int Quantity { get; init; }
    public WarehouseProductInfo Product { get; init; } = new();
}

public record WarehouseProductInfo
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public decimal SalePrice { get; init; }
    public WarehouseCategoryInfo? Category { get; init; }
    public WarehouseBrandInfo? Brand { get; init; }
}

public record WarehouseCategoryInfo
{
    public string Name { get; init; } = string.Empty;
}

public record WarehouseBrandInfo
{
    public string Name { get; init; } = string.Empty;
}

public record CreateWarehouseRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Location { get; init; }
}

public record UpdateWarehouseRequest
{
    public string? Name { get; init; }
    public string? Location { get; init; }
    public string? Status { get; init; }
}
