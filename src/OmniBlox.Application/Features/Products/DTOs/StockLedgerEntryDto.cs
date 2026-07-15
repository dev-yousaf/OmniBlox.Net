namespace OmniBlox.Application.Features.Products.DTOs;

public record StockLedgerEntryDto
{
    public Guid Id { get; init; }
    public int Quantity { get; init; }
    public int Balance { get; init; }
    public string Type { get; init; } = string.Empty;
    public string? Reference { get; init; }
    public string? Note { get; init; }
    public DateTime CreatedAt { get; init; }
    public Guid ProductId { get; init; }
}
