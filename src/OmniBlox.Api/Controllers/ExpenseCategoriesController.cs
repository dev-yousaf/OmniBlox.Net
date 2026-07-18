using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniBlox.Application.Features.ExpenseCategories.Commands;
using OmniBlox.Application.Features.ExpenseCategories.Queries;
using OmniBlox.Application.Features.Expenses.DTOs;

namespace OmniBlox.Api.Controllers;

[Route("expense-categories")]
[Authorize]
[ApiController]
public class ExpenseCategoriesController : ControllerBase
{
    private readonly IMediator _mediator;
    public ExpenseCategoriesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<List<ExpenseCategoryDto>>> GetAll()
    {
        return Ok(await _mediator.Send(new GetExpenseCategoriesQuery()));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExpenseCategoryDto>> GetById(Guid id)
    {
        return Ok(await _mediator.Send(new GetExpenseCategoryQuery { Id = id }));
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseCategoryDto>> Create(CreateExpenseCategoryDto request, CancellationToken ct)
    {
        var command = new CreateExpenseCategoryCommand
        {
            Name = request.Name,
            Description = request.Description,
        };
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExpenseCategoryDto>> Update(Guid id, UpdateExpenseCategoryDto request, CancellationToken ct)
    {
        var command = new UpdateExpenseCategoryCommand
        {
            Id = id,
            Name = request.Name,
            Description = request.Description,
        };
        return Ok(await _mediator.Send(command, ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteExpenseCategoryCommand { Id = id }, ct);
        return NoContent();
    }

    [HttpPost("bulk-delete")]
    public async Task<ActionResult<BulkDeleteResponse>> BulkDelete(BulkDeleteRequest request, CancellationToken ct)
    {
        var deleted = new List<string>();
        foreach (var id in request.Ids)
        {
            try
            {
                await _mediator.Send(new DeleteExpenseCategoryCommand { Id = id }, ct);
                deleted.Add(id.ToString());
            }
            catch { }
        }
        return Ok(new BulkDeleteResponse { Deleted = deleted, Count = deleted.Count });
    }
}

public record BulkDeleteResponse
{
    public List<string> Deleted { get; init; } = [];
    public int Count { get; init; }
}
