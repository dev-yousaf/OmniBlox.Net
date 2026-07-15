using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniBlox.Application.Features.Brands.Commands;
using OmniBlox.Application.Features.Brands.DTOs;
using OmniBlox.Application.Features.Brands.Queries;

namespace OmniBlox.Api.Controllers;

[Route("brands")]
[Authorize]
[ApiController]
public class BrandsController : ControllerBase
{
    private readonly IMediator _mediator;
    public BrandsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<List<BrandDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetBrandsQuery(), ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BrandDto>> GetById(Guid id, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetBrandByIdQuery { Id = id }, ct));
    }

    [HttpPost]
    public async Task<ActionResult<BrandDto>> Create(CreateBrandRequest request, CancellationToken ct)
    {
        var command = new CreateBrandCommand
        {
            Name = request.Name,
            Slug = request.Slug,
            ImageUrl = request.ImageUrl,
            Description = request.Description,
            Status = request.Status,
        };
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BrandDto>> Update(Guid id, UpdateBrandRequest request, CancellationToken ct)
    {
        var command = new UpdateBrandCommand
        {
            Id = id,
            Name = request.Name,
            Slug = request.Slug,
            ImageUrl = request.ImageUrl,
            Description = request.Description,
            Status = request.Status,
        };
        return Ok(await _mediator.Send(command, ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteBrandCommand { Id = id }, ct);
        return NoContent();
    }

    [HttpPost("bulk-delete")]
    public async Task<ActionResult<BulkDeleteResponse>> BulkDelete([FromBody] BulkDeleteRequest request, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new BulkDeleteBrandsCommand { Ids = request.Ids }, ct));
    }
}
