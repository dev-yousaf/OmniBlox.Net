using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniBlox.Application.Features.AuditLogs.DTOs;
using OmniBlox.Application.Features.AuditLogs.Queries;
using OmniBlox.Application.Features.Products.DTOs;
using OmniBlox.Application.Features.Products.Queries;
using OmniBlox.Application.Features.Team.Commands;
using OmniBlox.Application.Features.Team.DTOs;
using OmniBlox.Application.Features.Team.Queries;

namespace OmniBlox.Api.Controllers;

[Route("team")]
[Authorize]
[ApiController]
public class TeamController : ControllerBase
{
    private readonly IMediator _mediator;
    public TeamController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<TeamUserDto>> Create(CreateUserRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateUserCommand
        {
            Email = request.Email,
            Name = request.Name,
            Role = request.Role,
        }, ct);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(
        CancellationToken ct,
        [FromQuery] int? page = null, [FromQuery] int? limit = null,
        [FromQuery] string? search = null, [FromQuery] string? role = null)
    {
        if (page.HasValue)
        {
            return Ok(await _mediator.Send(new GetUsersQuery
            {
                Page = page.Value,
                Limit = limit ?? 20,
                Search = search,
                Role = role,
            }, ct));
        }

        return Ok(await _mediator.Send(new GetAllUsersQuery(), ct));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<TeamStatsDto>> GetStats(CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetTeamStatsQuery(), ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TeamUserDto>> GetById(Guid id, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetUserQuery { Id = id }, ct));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TeamUserDto>> Update(Guid id, UpdateUserRequest request, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new UpdateUserCommand
        {
            Id = id,
            Email = request.Email,
            Name = request.Name,
            Role = request.Role,
        }, ct));
    }

    [HttpPut("{id:guid}/password")]
    public async Task<ActionResult> ChangePassword(Guid id, ChangePasswordRequest request, CancellationToken ct)
    {
        await _mediator.Send(new ChangePasswordCommand
        {
            Id = id,
            CurrentPassword = request.CurrentPassword,
            NewPassword = request.NewPassword,
        }, ct);
        return Ok(new { message = "Password changed." });
    }

    [HttpGet("{id:guid}/products")]
    public async Task<ActionResult<List<ProductDto>>> GetUserProducts(Guid id, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetUserProductsQuery { UserId = id }, ct));
    }

    [HttpGet("{id:guid}/audit-logs")]
    public async Task<ActionResult<AuditLogListResponse>> GetUserAuditLogs(
        Guid id, CancellationToken ct,
        [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        return Ok(await _mediator.Send(new GetAuditLogsQuery
        {
            UserId = id,
            Page = page,
            Limit = limit,
        }, ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteUserCommand { Id = id }, ct);
        return Ok(new { message = "User deleted." });
    }
}
