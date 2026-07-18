using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniBlox.Api.Controllers.Requests;
using OmniBlox.Application.Features.Inventory.Commands;
using OmniBlox.Application.Features.Inventory.DTOs;
using OmniBlox.Application.Features.Inventory.Queries;

namespace OmniBlox.Api.Controllers;

[Route("inventory")]
[Authorize]
[ApiController]
public class InventoryController : ControllerBase
{
    private readonly IMediator _mediator;
    public InventoryController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<InventoryListResponse>> GetAll(
        CancellationToken ct,
        [FromQuery] int page = 1, [FromQuery] int limit = 20,
        [FromQuery] string? search = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] string? filter = null)
    {
        return Ok(await _mediator.Send(new GetInventoriesQuery
        {
            Page = page,
            Limit = limit,
            Search = search,
            WarehouseId = warehouseId,
            Filter = filter,
        }, ct));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<InventoryStatsDto>> GetStats(CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetInventoryStatsQuery(), ct));
    }

    [HttpGet("product/{productId:guid}")]
    public async Task<ActionResult<List<InventoryDto>>> GetByProduct(Guid productId, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetInventoryByProductQuery { ProductId = productId }, ct));
    }

    [HttpPut("{productId:guid}/{warehouseId:guid}")]
    public async Task<ActionResult<InventoryDto>> UpdateInventory(Guid productId, Guid warehouseId, [FromBody] UpdateInventoryRequest request, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new UpdateInventoryCommand
        {
            ProductId = productId,
            WarehouseId = warehouseId,
            Quantity = request.Quantity,
            Notes = request.Notes,
        }, ct));
    }

    [HttpPost("transfers")]
    public async Task<ActionResult<StockTransferDto>> TransferStock([FromBody] TransferStockRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new TransferStockCommand
        {
            ProductId = request.ProductId,
            FromWarehouseId = request.FromWarehouseId,
            ToWarehouseId = request.ToWarehouseId,
            Quantity = request.Quantity,
            Note = request.Note,
        }, ct);
        return Ok(result);
    }

    [HttpPost("transfers/bulk")]
    public async Task<ActionResult<StockTransferDto>> BulkTransferStock([FromBody] BulkTransferRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new BulkTransferStockCommand
        {
            FromWarehouseId = request.FromWarehouseId,
            ToWarehouseId = request.ToWarehouseId,
            Items = request.Items,
            Note = request.Note,
        }, ct);
        return Ok(result);
    }

    [HttpGet("transfers")]
    public async Task<ActionResult<TransferListResponse>> GetTransfers(
        CancellationToken ct,
        [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        return Ok(await _mediator.Send(new GetStockTransfersQuery
        {
            Page = page,
            Limit = limit,
        }, ct));
    }

    [HttpGet("transfers/{id:guid}")]
    public async Task<ActionResult<StockTransferDto>> GetTransferById(Guid id, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetStockTransferByIdQuery { Id = id }, ct));
    }

    [HttpPost("adjustments")]
    public async Task<ActionResult<StockAdjustmentDto>> CreateAdjustment([FromBody] CreateStockAdjustmentRequest request, CancellationToken ct)
    {
        var command = new CreateStockAdjustmentCommand
        {
            Type = request.Type,
            Items = request.Items,
            Notes = request.Notes,
            DocumentUrl = request.DocumentUrl,
        };
        return Ok(await _mediator.Send(command, ct));
    }

    [HttpGet("adjustments")]
    public async Task<ActionResult<AdjustmentListResponse>> GetAdjustments(
        CancellationToken ct,
        [FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        return Ok(await _mediator.Send(new GetStockAdjustmentsQuery
        {
            Page = page,
            Limit = limit,
        }, ct));
    }

    [HttpGet("adjustments/{id:guid}")]
    public async Task<ActionResult<StockAdjustmentDto>> GetAdjustmentById(Guid id, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetStockAdjustmentByIdQuery { Id = id }, ct));
    }

    [HttpPost("backfill")]
    public async Task<ActionResult<BackfillResult>> Backfill(CancellationToken ct)
    {
        return Ok(await _mediator.Send(new BackfillInventoryRecordsCommand(), ct));
    }

    [HttpGet("warehouses/{warehouseId:guid}/inventory")]
    public async Task<ActionResult<WarehouseInventoryDto>> GetWarehouseInventory(Guid warehouseId, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetWarehouseInventoryQuery { WarehouseId = warehouseId }, ct));
    }
}
