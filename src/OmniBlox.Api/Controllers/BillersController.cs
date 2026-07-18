using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniBlox.Application.Features.Billers.Commands;
using OmniBlox.Application.Features.Billers.DTOs;
using OmniBlox.Application.Features.Billers.Queries;

namespace OmniBlox.Api.Controllers;

[Route("billers")]
[Authorize]
[ApiController]
public class BillersController : ControllerBase
{
    private readonly IMediator _mediator;
    public BillersController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<List<BillerDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetBillersQuery(), ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BillerDto>> GetById(Guid id, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetBillerByIdQuery { Id = id }, ct));
    }

    [HttpPost]
    public async Task<ActionResult<BillerDto>> Create(CreateBillerRequest request, CancellationToken ct)
    {
        var command = new CreateBillerCommand
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            Status = request.Status,
        };
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BillerDto>> Update(Guid id, UpdateBillerRequest request, CancellationToken ct)
    {
        var command = new UpdateBillerCommand
        {
            Id = id,
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            Status = request.Status,
        };
        return Ok(await _mediator.Send(command, ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteBillerCommand { Id = id }, ct);
        return NoContent();
    }

    [HttpPost("bulk-delete")]
    public async Task<ActionResult<BulkDeleteResponse>> BulkDelete([FromBody] BulkDeleteRequest request, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new BulkDeleteBillersCommand { Ids = request.Ids }, ct));
    }
}
