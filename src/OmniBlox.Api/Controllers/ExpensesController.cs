using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniBlox.Application.Features.Expenses.Commands;
using OmniBlox.Application.Features.Expenses.DTOs;
using OmniBlox.Application.Features.Expenses.Queries;

namespace OmniBlox.Api.Controllers;

[Route("expenses")]
[Authorize]
[ApiController]
public class ExpensesController : ControllerBase
{
    private readonly IMediator _mediator;
    public ExpensesController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<List<ExpenseDto>>> GetAll(
        [FromQuery] string? search, [FromQuery] string? status,
        [FromQuery] Guid? categoryId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        return Ok(await _mediator.Send(new GetExpensesQuery
        {
            Search = search, Status = status,
            CategoryId = categoryId, FromDate = fromDate, ToDate = toDate,
        }));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ExpenseStatsDto>> GetStats()
    {
        return Ok(await _mediator.Send(new GetExpenseStatsQuery()));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExpenseDto>> GetById(Guid id)
    {
        return Ok(await _mediator.Send(new GetExpenseQuery { Id = id }));
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> Create(CreateExpenseDto request, CancellationToken ct)
    {
        var command = new CreateExpenseCommand
        {
            Reference = request.Reference,
            Amount = request.Amount,
            ExpenseDate = request.ExpenseDate,
            Description = request.Description,
            Vendor = request.Vendor,
            PaymentMethod = request.PaymentMethod,
            CategoryId = request.CategoryId,
            PurchaseOrderId = request.PurchaseOrderId,
            SaleId = request.SaleId,
        };
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExpenseDto>> Update(Guid id, UpdateExpenseDto request, CancellationToken ct)
    {
        var command = new UpdateExpenseCommand
        {
            Id = id,
            Reference = request.Reference,
            Amount = request.Amount,
            ExpenseDate = request.ExpenseDate,
            Description = request.Description,
            Vendor = request.Vendor,
            PaymentMethod = request.PaymentMethod,
            CategoryId = request.CategoryId,
        };
        return Ok(await _mediator.Send(command, ct));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ExpenseDto>> UpdateStatus(Guid id, UpdateExpenseStatusDto request, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new UpdateExpenseStatusCommand
        {
            Id = id,
            Status = request.Status,
        }, ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteExpenseCommand { Id = id }, ct);
        return NoContent();
    }
}
