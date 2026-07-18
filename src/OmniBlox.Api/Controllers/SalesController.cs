using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniBlox.Application.Features.Sales.Commands;
using OmniBlox.Application.Features.Sales.DTOs;
using OmniBlox.Application.Features.Sales.Queries;

namespace OmniBlox.Api.Controllers;

[Route("sales")]
[Authorize]
[ApiController]
public class SalesController : ControllerBase
{
    private readonly IMediator _mediator;
    public SalesController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<SaleDetailDto>> Create(CreateSaleRequest request, CancellationToken ct)
    {
        var command = new CreateSaleCommand
        {
            CustomerId = request.CustomerId,
            WarehouseId = request.WarehouseId,
            SaleDate = request.SaleDate,
            DueDate = request.DueDate,
            Status = request.Status,
            PaymentStatus = request.PaymentStatus,
            PaymentMethod = request.PaymentMethod,
            TaxRate = request.TaxRate,
            Discount = request.Discount,
            Notes = request.Notes,
            ShippingAddress = request.ShippingAddress,
            Items = request.Items.Select(i => new CreateSaleItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
            }).ToList(),
        };
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SaleDetailDto>> GetById(Guid id, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetSaleQuery { Id = id }, ct));
    }

    [HttpGet]
    public async Task<ActionResult<SalesListResponse>> GetAll(
        CancellationToken ct,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? paymentStatus = null,
        [FromQuery] Guid? warehouseId = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] Guid? productId = null)
    {
        return Ok(await _mediator.Send(new GetSalesQuery
        {
            Page = page, PageSize = pageSize, Search = search,
            Status = status, PaymentStatus = paymentStatus,
            WarehouseId = warehouseId, DateFrom = dateFrom,
            DateTo = dateTo, ProductId = productId,
        }, ct));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<SalesStatsDto>> GetStats(CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetSalesStatsQuery(), ct));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SaleDetailDto>> Update(Guid id, UpdateSaleRequest request, CancellationToken ct)
    {
        var command = new UpdateSaleCommand
        {
            Id = id,
            CustomerId = request.CustomerId,
            WarehouseId = request.WarehouseId,
            SaleDate = request.SaleDate,
            DueDate = request.DueDate,
            Status = request.Status!,
            PaymentStatus = request.PaymentStatus,
            PaymentMethod = request.PaymentMethod,
            TaxRate = request.TaxRate,
            Discount = request.Discount,
            Notes = request.Notes,
            ShippingAddress = request.ShippingAddress,
            Items = request.Items.Select(i => new CreateSaleItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
            }).ToList(),
        };
        return Ok(await _mediator.Send(command, ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteSaleCommand { Id = id }, ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/mark-paid")]
    public async Task<ActionResult<SaleDetailDto>> MarkPaid(Guid id, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new MarkSalePaidCommand { Id = id }, ct));
    }
}
