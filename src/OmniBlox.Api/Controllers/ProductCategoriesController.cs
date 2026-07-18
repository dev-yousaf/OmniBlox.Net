using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniBlox.Api.Controllers.Requests;
using OmniBlox.Application.Features.ProductCategories.Commands;
using OmniBlox.Application.Features.ProductCategories.DTOs;
using OmniBlox.Application.Features.ProductCategories.Queries;

namespace OmniBlox.Api.Controllers;

[Route("product-categories")]
[Authorize]
[ApiController]
public class ProductCategoriesController : ControllerBase
{
    private readonly IMediator _mediator;
    public ProductCategoriesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<List<ProductCategoryDto>>> GetAll(CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetProductCategoriesQuery(), ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductCategoryDto>> GetById(Guid id, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetProductCategoryByIdQuery { Id = id }, ct));
    }

    [HttpPost]
    public async Task<ActionResult<ProductCategoryDto>> Create(CreateProductCategoryRequest request, CancellationToken ct)
    {
        var command = new CreateProductCategoryCommand
        {
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            Status = request.Status,
        };
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductCategoryDto>> Update(Guid id, UpdateProductCategoryRequest request, CancellationToken ct)
    {
        var command = new UpdateProductCategoryCommand
        {
            Id = id,
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            Status = request.Status,
        };
        return Ok(await _mediator.Send(command, ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<DeleteCategoryResponse>> Delete(Guid id, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new DeleteProductCategoryCommand { Id = id }, ct));
    }

    [HttpPost("bulk-delete")]
    public async Task<ActionResult<BulkDeleteResponse>> BulkDelete([FromBody] BulkDeleteRequest request, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new BulkDeleteProductCategoriesCommand { Ids = request.Ids }, ct));
    }
}
