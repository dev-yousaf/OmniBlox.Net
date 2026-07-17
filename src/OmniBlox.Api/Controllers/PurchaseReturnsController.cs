using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniBlox.Application.Features.PurchaseReturns.Commands;
using OmniBlox.Application.Features.PurchaseReturns.DTOs;
using OmniBlox.Application.Features.PurchaseReturns.Queries;

namespace OmniBlox.Api.Controllers;

[Route("purchase-returns")]
[Authorize]
[ApiController]
public class PurchaseReturnsController : ControllerBase
{
    private readonly IMediator _mediator;
    public PurchaseReturnsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<PurchaseReturnDetailDto>> Create(CreatePurchaseReturnRequest request, CancellationToken ct)
    {
        var command = new CreatePurchaseReturnCommand
        {
            PurchaseOrderId = request.PurchaseOrderId,
            SupplierId = request.SupplierId,
            WarehouseId = request.WarehouseId,
            ReturnDate = request.ReturnDate,
            Reason = request.Reason,
            Items = request.Items.Select(i => new CreatePurchaseReturnItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitCost = i.UnitCost,
                PurchaseOrderItemId = i.PurchaseOrderItemId,
            }).ToList(),
        };
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PurchaseReturnDetailDto>> GetById(Guid id, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetPurchaseReturnQuery { Id = id }, ct));
    }

    [HttpGet]
    public async Task<ActionResult<PurchaseReturnListResponse>> GetAll(
        CancellationToken ct,
        [FromQuery] int page = 1, [FromQuery] int limit = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? supplierId = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        return Ok(await _mediator.Send(new GetPurchaseReturnsQuery
        {
            Page = page, Limit = limit, Search = search,
            Status = status, SupplierId = supplierId,
            WarehouseId = warehouseId, FromDate = fromDate, ToDate = toDate,
        }, ct));
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<PurchaseReturnDetailDto>> UpdateStatus(Guid id, UpdatePurchaseReturnStatusRequest request, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new UpdatePurchaseReturnStatusCommand
        {
            Id = id,
            Status = request.Status,
        }, ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeletePurchaseReturnCommand { Id = id }, ct);
        return NoContent();
    }
}
