using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniBlox.Application.Features.Quotations.Commands;
using OmniBlox.Application.Features.Quotations.DTOs;
using OmniBlox.Application.Features.Quotations.Queries;

namespace OmniBlox.Api.Controllers;

[Route("quotations")]
[Authorize]
[ApiController]
public class QuotationsController : ControllerBase
{
    private readonly IMediator _mediator;
    public QuotationsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<QuotationDetailDto>> Create(CreateQuotationRequest request, CancellationToken ct)
    {
        var command = new CreateQuotationCommand
        {
            CustomerId = request.CustomerId,
            QuoteDate = request.QuoteDate,
            ExpiryDate = request.ExpiryDate,
            Status = request.Status ?? "DRAFT",
            Notes = request.Notes,
            Items = request.Items.Select(i => new CreateQuotationItemCommand
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
    public async Task<ActionResult<QuotationDetailDto>> GetById(Guid id, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetQuotationQuery { Id = id }, ct));
    }

    [HttpGet]
    public async Task<ActionResult<QuotationListResponse>> GetAll(
        CancellationToken ct,
        [FromQuery] int page = 1, [FromQuery] int limit = 20,
        [FromQuery] string? status = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        return Ok(await _mediator.Send(new GetQuotationsQuery
        {
            Page = page, Limit = limit, Status = status,
            CustomerId = customerId, DateFrom = dateFrom, DateTo = dateTo,
        }, ct));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<QuotationDetailDto>> Update(Guid id, UpdateQuotationRequest request, CancellationToken ct)
    {
        var command = new UpdateQuotationCommand
        {
            Id = id,
            CustomerId = request.CustomerId,
            QuoteDate = request.QuoteDate,
            ExpiryDate = request.ExpiryDate,
            Status = request.Status,
            Notes = request.Notes,
            Items = request.Items.Select(i => new CreateQuotationItemCommand
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
        await _mediator.Send(new DeleteQuotationCommand { Id = id }, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/convert-to-sale")]
    public async Task<ActionResult<QuotationSaleResult>> ConvertToSale(Guid id, [FromBody] ConvertQuotationRequest request, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new ConvertQuotationToSaleCommand
        {
            QuotationId = id,
            WarehouseId = request.WarehouseId,
            Status = request.Status ?? "COMPLETED",
            PaymentStatus = request.PaymentStatus ?? "PENDING",
            PaymentMethod = request.PaymentMethod,
            SaleDate = request.SaleDate,
            DueDate = request.DueDate,
            Notes = request.Notes,
            ShippingAddress = request.ShippingAddress,
        }, ct));
    }
}

public record ConvertQuotationRequest
{
    public Guid WarehouseId { get; init; }
    public DateTime SaleDate { get; init; }
    public DateTime DueDate { get; init; }
    public string? Status { get; init; }
    public string? PaymentStatus { get; init; }
    public string? PaymentMethod { get; init; }
    public string? Notes { get; init; }
    public string? ShippingAddress { get; init; }
}
