using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniBlox.Application.Features.AuditLogs.DTOs;
using OmniBlox.Application.Features.AuditLogs.Queries;

namespace OmniBlox.Api.Controllers;

[Route("audit-logs")]
[Authorize]
[ApiController]
public class AuditLogsController : ControllerBase
{
    private readonly IMediator _mediator;
    public AuditLogsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<AuditLogListResponse>> GetAll(
        CancellationToken ct,
        [FromQuery] int page = 1, [FromQuery] int limit = 20,
        [FromQuery] Guid? userId = null)
    {
        return Ok(await _mediator.Send(new GetAuditLogsQuery
        {
            Page = page,
            Limit = limit,
            UserId = userId,
        }, ct));
    }
}
