using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniBlox.Application.Features.Purchases.Commands;
using OmniBlox.Application.Features.Purchases.DTOs;
using OmniBlox.Application.Features.Purchases.Queries;

namespace OmniBlox.Api.Controllers;

[Route("purchases")]
[Authorize]
[ApiController]
public class PurchasesController : ControllerBase
{
    private readonly IMediator _mediator;
    public PurchasesController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<PurchaseOrderDetailDto>> Create(CreatePurchaseOrderRequest request, CancellationToken ct)
    {
        var command = new CreatePurchaseOrderCommand
        {
            SupplierId = request.SupplierId,
            OrderDate = request.OrderDate,
            ReferenceNumber = request.ReferenceNumber,
            BillNumber = request.BillNumber,
            BillDate = request.BillDate,
            DueDate = request.DueDate,
            PaymentStatus = request.PaymentStatus,
            PaymentMethod = request.PaymentMethod,
            Status = request.Status,
            Notes = request.Notes,
            WarehouseId = request.WarehouseId,
            Items = request.Items.Select(i => new CreatePurchaseItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitCost = i.UnitCost,
            }).ToList(),
        };
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PurchaseOrderDetailDto>> GetById(Guid id, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetPurchaseQuery { Id = id }, ct));
    }

    [HttpGet]
    public async Task<ActionResult<PurchaseListResponse>> GetAll(
        CancellationToken ct,
        [FromQuery] int page = 1, [FromQuery] int limit = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? paymentStatus = null,
        [FromQuery] Guid? supplierId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        return Ok(await _mediator.Send(new GetPurchasesQuery
        {
            Page = page, Limit = limit, Search = search,
            Status = status, PaymentStatus = paymentStatus,
            SupplierId = supplierId, FromDate = fromDate, ToDate = toDate,
        }, ct));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<PurchaseOrderStatsDto>> GetStats(CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetPurchaseStatsQuery(), ct));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PurchaseOrderDetailDto>> Update(Guid id, UpdatePurchaseOrderRequest request, CancellationToken ct)
    {
        var command = new UpdatePurchaseOrderCommand
        {
            Id = id,
            SupplierId = request.SupplierId,
            OrderDate = request.OrderDate,
            BillNumber = request.BillNumber,
            BillDate = request.BillDate,
            DueDate = request.DueDate,
            PaymentStatus = request.PaymentStatus,
            PaymentMethod = request.PaymentMethod,
            Notes = request.Notes,
            WarehouseId = request.WarehouseId,
            Items = request.Items.Select(i => new CreatePurchaseItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitCost = i.UnitCost,
            }).ToList(),
        };
        return Ok(await _mediator.Send(command, ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeletePurchaseOrderCommand { Id = id }, ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/receive")]
    public async Task<ActionResult<PurchaseOrderDetailDto>> Receive(Guid id, [FromBody] ReceivePurchaseRequest request, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new ReceivePurchaseOrderCommand
        {
            Id = id,
            WarehouseId = request.WarehouseId,
        }, ct));
    }

    [HttpPatch("{id:guid}/mark-paid")]
    public async Task<ActionResult<PurchaseOrderDetailDto>> MarkPaid(Guid id, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new MarkPurchasePaidCommand { Id = id }, ct));
    }
}

public record ReceivePurchaseRequest
{
    public Guid WarehouseId { get; init; }
}
