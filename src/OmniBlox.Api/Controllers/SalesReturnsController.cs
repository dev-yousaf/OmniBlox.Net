using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniBlox.Application.Features.SalesReturns.Commands;
using OmniBlox.Application.Features.SalesReturns.DTOs;
using OmniBlox.Application.Features.SalesReturns.Queries;

namespace OmniBlox.Api.Controllers;

[Route("sales-returns")]
[Authorize]
[ApiController]
public class SalesReturnsController : ControllerBase
{
    private readonly IMediator _mediator;
    public SalesReturnsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<SalesReturnDetailDto>> Create(CreateSalesReturnRequest request, CancellationToken ct)
    {
        var command = new CreateSalesReturnCommand
        {
            SaleId = request.SaleId,
            WarehouseId = request.WarehouseId,
            ReturnDate = request.ReturnDate,
            Reason = request.Reason,
            Items = request.Items.Select(i => new CreateSalesReturnItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                SaleItemId = i.SaleItemId,
            }).ToList(),
        };
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SalesReturnDetailDto>> GetById(Guid id, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetSalesReturnQuery { Id = id }, ct));
    }

    [HttpGet]
    public async Task<ActionResult<SalesReturnsListResponse>> GetAll(
        CancellationToken ct,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        return Ok(await _mediator.Send(new GetSalesReturnsQuery
        {
            Page = page, PageSize = pageSize, Search = search,
            Status = status, WarehouseId = warehouseId,
            DateFrom = dateFrom, DateTo = dateTo,
        }, ct));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SalesReturnDetailDto>> Update(Guid id, UpdateSalesReturnRequest request, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new UpdateSalesReturnCommand
        {
            Id = id,
            WarehouseId = request.WarehouseId,
            SaleId = request.SaleId,
            ReturnDate = request.ReturnDate,
            Reason = request.Reason,
            Items = request.Items.Select(i => new CreateSalesReturnItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                SaleItemId = i.SaleItemId,
            }).ToList(),
        }, ct));
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<SalesReturnDetailDto>> UpdateStatus(Guid id, UpdateSalesReturnStatusRequest request, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new UpdateSalesReturnStatusCommand
        {
            Id = id,
            Status = request.Status,
        }, ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteSalesReturnCommand { Id = id }, ct);
        return NoContent();
    }
}
