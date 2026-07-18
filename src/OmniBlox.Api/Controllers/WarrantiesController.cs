using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniBlox.Api.Controllers.Requests;
using OmniBlox.Application.Features.Warranties.Commands;
using OmniBlox.Application.Features.Warranties.DTOs;
using OmniBlox.Application.Features.Warranties.Queries;

namespace OmniBlox.Api.Controllers;

[Route("warranties")]
[Authorize]
[ApiController]
public class WarrantiesController : ControllerBase
{
    private readonly IMediator _mediator;
    public WarrantiesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<List<WarrantyDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetWarrantiesQuery(), ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WarrantyDto>> GetById(Guid id, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetWarrantyByIdQuery { Id = id }, ct));
    }

    [HttpPost]
    public async Task<ActionResult<WarrantyDto>> Create(CreateWarrantyRequest request, CancellationToken ct)
    {
        var command = new CreateWarrantyCommand
        {
            Name = request.Name,
            Duration = request.Duration,
            DurationType = request.DurationType,
            Description = request.Description,
            Status = request.Status,
        };
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<WarrantyDto>> Update(Guid id, UpdateWarrantyRequest request, CancellationToken ct)
    {
        var command = new UpdateWarrantyCommand
        {
            Id = id,
            Name = request.Name,
            Duration = request.Duration,
            DurationType = request.DurationType,
            Description = request.Description,
            Status = request.Status,
        };
        return Ok(await _mediator.Send(command, ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteWarrantyCommand { Id = id }, ct);
        return NoContent();
    }

    [HttpPost("bulk-delete")]
    public async Task<ActionResult<BulkDeleteResponse>> BulkDelete([FromBody] BulkDeleteRequest request, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new BulkDeleteWarrantiesCommand { Ids = request.Ids }, ct));
    }
}
