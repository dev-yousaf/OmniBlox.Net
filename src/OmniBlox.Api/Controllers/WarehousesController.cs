using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniBlox.Api.Controllers.Requests;
using OmniBlox.Application.Features.Brands.Commands;
using OmniBlox.Application.Features.Warehouses.Commands;
using OmniBlox.Application.Features.Warehouses.DTOs;
using OmniBlox.Application.Features.Warehouses.Queries;

namespace OmniBlox.Api.Controllers;

[Route("inventory/warehouses")]
[Authorize]
[ApiController]
public class WarehousesController : ControllerBase
{
    private readonly IMediator _mediator;
    public WarehousesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<List<WarehouseDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetWarehousesQuery(), ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WarehouseDto>> GetById(Guid id, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetWarehouseByIdQuery { Id = id }, ct));
    }

    [HttpPost]
    public async Task<ActionResult<WarehouseDto>> Create(CreateWarehouseRequest request, CancellationToken ct)
    {
        var command = new CreateWarehouseCommand
        {
            Name = request.Name,
            Location = request.Location,
        };
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<WarehouseDto>> Update(Guid id, UpdateWarehouseRequest request, CancellationToken ct)
    {
        var command = new UpdateWarehouseCommand
        {
            Id = id,
            Name = request.Name,
            Location = request.Location,
            Status = request.Status,
        };
        return Ok(await _mediator.Send(command, ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteWarehouseCommand { Id = id }, ct);
        return NoContent();
    }

    [HttpPost("bulk-delete")]
    public async Task<ActionResult<BulkDeleteResponse>> BulkDelete([FromBody] BulkDeleteRequest request, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new BulkDeleteWarehousesCommand { Ids = request.Ids }, ct));
    }
}
