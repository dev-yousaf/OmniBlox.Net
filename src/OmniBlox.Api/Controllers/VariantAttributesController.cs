using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniBlox.Application.Features.VariantAttributes.Commands;
using OmniBlox.Application.Features.VariantAttributes.DTOs;
using OmniBlox.Application.Features.VariantAttributes.Queries;

namespace OmniBlox.Api.Controllers;

[Route("variant-attributes")]
[Authorize]
[ApiController]
public class VariantAttributesController : ControllerBase
{
    private readonly IMediator _mediator;
    public VariantAttributesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<List<VariantAttributeDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetVariantAttributesQuery(), ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VariantAttributeDto>> GetById(Guid id, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetVariantAttributeByIdQuery { Id = id }, ct));
    }

    [HttpPost]
    public async Task<ActionResult<VariantAttributeDto>> Create(CreateVariantAttributeRequest request, CancellationToken ct)
    {
        var command = new CreateVariantAttributeCommand
        {
            Name = request.Name,
            Slug = request.Slug,
            Values = request.Values,
            Description = request.Description,
            Status = request.Status,
        };
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<VariantAttributeDto>> Update(Guid id, UpdateVariantAttributeRequest request, CancellationToken ct)
    {
        var command = new UpdateVariantAttributeCommand
        {
            Id = id,
            Name = request.Name,
            Slug = request.Slug,
            Values = request.Values,
            Description = request.Description,
            Status = request.Status,
        };
        return Ok(await _mediator.Send(command, ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteVariantAttributeCommand { Id = id }, ct);
        return NoContent();
    }

    [HttpPost("bulk-delete")]
    public async Task<ActionResult<BulkDeleteResponse>> BulkDelete([FromBody] BulkDeleteRequest request, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new BulkDeleteVariantAttributesCommand { Ids = request.Ids }, ct));
    }
}
