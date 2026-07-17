using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniBlox.Application.Features.Customers.Commands;
using OmniBlox.Application.Features.Customers.DTOs;
using OmniBlox.Application.Features.Customers.Queries;

namespace OmniBlox.Api.Controllers;

[Route("customers")]
[Authorize]
[ApiController]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;
    public CustomersController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(CreateCustomerRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateCustomerCommand
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
        }, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> GetById(Guid id, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetCustomerQuery { Id = id }, ct));
    }

    [HttpGet]
    public async Task<ActionResult<CustomerListResponse>> GetAll(
        CancellationToken ct,
        [FromQuery] int page = 1, [FromQuery] int limit = 20,
        [FromQuery] string? search = null)
    {
        return Ok(await _mediator.Send(new GetCustomersQuery
        {
            Page = page, Limit = limit, Search = search,
        }, ct));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> Update(Guid id, UpdateCustomerRequest request, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new UpdateCustomerCommand
        {
            Id = id, Name = request.Name, Email = request.Email,
            Phone = request.Phone, Address = request.Address,
        }, ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteCustomerCommand { Id = id }, ct);
        return NoContent();
    }
}
