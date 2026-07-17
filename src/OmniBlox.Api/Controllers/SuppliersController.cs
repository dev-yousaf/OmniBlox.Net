using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniBlox.Application.Features.Suppliers.Commands;
using OmniBlox.Application.Features.Suppliers.DTOs;
using OmniBlox.Application.Features.Suppliers.Queries;

namespace OmniBlox.Api.Controllers;

[Route("suppliers")]
[Authorize]
[ApiController]
public class SuppliersController : ControllerBase
{
    private readonly IMediator _mediator;
    public SuppliersController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<SupplierDto>> Create(CreateSupplierRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateSupplierCommand
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
        }, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SupplierDto>> GetById(Guid id, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetSupplierQuery { Id = id }, ct));
    }

    [HttpGet]
    public async Task<ActionResult<SupplierListResponse>> GetAll(
        CancellationToken ct,
        [FromQuery] int page = 1, [FromQuery] int limit = 20,
        [FromQuery] string? search = null)
    {
        return Ok(await _mediator.Send(new GetSuppliersQuery
        {
            Page = page, Limit = limit, Search = search,
        }, ct));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SupplierDto>> Update(Guid id, UpdateSupplierRequest request, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new UpdateSupplierCommand
        {
            Id = id, Name = request.Name, Email = request.Email,
            Phone = request.Phone, Address = request.Address,
        }, ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteSupplierCommand { Id = id }, ct);
        return NoContent();
    }
}
