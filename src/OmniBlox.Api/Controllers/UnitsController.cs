using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniBlox.Application.Features.Units.Commands;
using OmniBlox.Application.Features.Units.DTOs;
using OmniBlox.Application.Features.Units.Queries;

namespace OmniBlox.Api.Controllers;

[Route("units")]
[Authorize]
[ApiController]
public class UnitsController : ControllerBase
{
    private readonly IMediator _mediator;
    public UnitsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<List<UnitDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetUnitsQuery(), ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UnitDto>> GetById(Guid id, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetUnitByIdQuery { Id = id }, ct));
    }

    [HttpPost]
    public async Task<ActionResult<UnitDto>> Create(CreateUnitRequest request, CancellationToken ct)
    {
        var command = new CreateUnitCommand
        {
            Name = request.Name,
            ShortName = request.ShortName,
            Slug = request.Slug,
            Description = request.Description,
            Status = request.Status,
        };
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UnitDto>> Update(Guid id, UpdateUnitRequest request, CancellationToken ct)
    {
        var command = new UpdateUnitCommand
        {
            Id = id,
            Name = request.Name,
            ShortName = request.ShortName,
            Slug = request.Slug,
            Description = request.Description,
            Status = request.Status,
        };
        return Ok(await _mediator.Send(command, ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteUnitCommand { Id = id }, ct);
        return NoContent();
    }

    [HttpPost("bulk-delete")]
    public async Task<ActionResult<BulkDeleteResponse>> BulkDelete([FromBody] BulkDeleteRequest request, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new BulkDeleteUnitsCommand { Ids = request.Ids }, ct));
    }
}
