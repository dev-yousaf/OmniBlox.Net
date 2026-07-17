using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;

namespace OmniBlox.Application.Common.Interfaces;

public record RecordMovementArgs
{
    public Guid ProductId { get; init; }
    public Guid WarehouseId { get; init; }
    public MovementType MovementType { get; init; }
    public int Quantity { get; init; }
    public string? ReferenceType { get; init; }
    public Guid? ReferenceId { get; init; }
    public Guid UserId { get; init; }
}

public record RecordTransferArgs
{
    public Guid ProductId { get; init; }
    public Guid FromWarehouseId { get; init; }
    public Guid ToWarehouseId { get; init; }
    public int Quantity { get; init; }
    public string? ReferenceType { get; init; }
    public Guid? ReferenceId { get; init; }
    public Guid UserId { get; init; }
}

public interface IStockService
{
    Task<StockMovement> RecordMovementAsync(RecordMovementArgs args, CancellationToken ct = default);
    Task<(StockMovement OutMovement, StockMovement InMovement)> RecordTransferAsync(
        RecordTransferArgs args, CancellationToken ct = default);
}
