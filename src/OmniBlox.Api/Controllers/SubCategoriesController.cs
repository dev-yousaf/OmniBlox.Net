using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniBlox.Api.Controllers.Requests;
using OmniBlox.Application.Features.SubCategories.Commands;
using OmniBlox.Application.Features.SubCategories.DTOs;
using OmniBlox.Application.Features.SubCategories.Queries;

namespace OmniBlox.Api.Controllers;

[Route("sub-categories")]
[Authorize]
[ApiController]
public class SubCategoriesController : ControllerBase
{
    private readonly IMediator _mediator;
    public SubCategoriesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<List<SubCategoryDto>>> GetAll([FromQuery] Guid? categoryId, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetSubCategoriesQuery { CategoryId = categoryId }, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SubCategoryDto>> GetById(Guid id, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetSubCategoryByIdQuery { Id = id }, ct));
    }

    [HttpPost]
    public async Task<ActionResult<SubCategoryDto>> Create(CreateSubCategoryRequest request, CancellationToken ct)
    {
        var command = new CreateSubCategoryCommand
        {
            Name = request.Name,
            CategoryId = request.CategoryId,
            Slug = request.Slug,
            Code = request.Code,
            ImageUrl = request.ImageUrl,
            Description = request.Description,
            Status = request.Status,
        };
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SubCategoryDto>> Update(Guid id, UpdateSubCategoryRequest request, CancellationToken ct)
    {
        var command = new UpdateSubCategoryCommand
        {
            Id = id,
            Name = request.Name,
            CategoryId = request.CategoryId,
            Slug = request.Slug,
            Code = request.Code,
            ImageUrl = request.ImageUrl,
            Description = request.Description,
            Status = request.Status,
        };
        return Ok(await _mediator.Send(command, ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteSubCategoryCommand { Id = id }, ct);
        return NoContent();
    }

    [HttpPost("bulk-delete")]
    public async Task<ActionResult<BulkDeleteResponse>> BulkDelete([FromBody] BulkDeleteRequest request, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new BulkDeleteSubCategoriesCommand { Ids = request.Ids }, ct));
    }
}
