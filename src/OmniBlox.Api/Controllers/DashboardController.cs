using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniBlox.Application.Features.Dashboard.DTOs;
using OmniBlox.Application.Features.Dashboard.Queries;

namespace OmniBlox.Api.Controllers;

[Route("dashboard")]
[Authorize]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<DashboardDataDto>> GetDashboard(CancellationToken ct, [FromQuery] string period = "1Y")
    {
        return Ok(await _mediator.Send(new GetDashboardQuery { Period = period }, ct));
    }

    [HttpGet("sales-stats")]
    public async Task<ActionResult<SalesStatsResponseDto>> GetSalesStats(CancellationToken ct, [FromQuery] string period = "1Y")
    {
        return Ok(await _mediator.Send(new GetSalesStatsQuery { Period = period }, ct));
    }

    [HttpGet("top-selling")]
    public async Task<ActionResult<List<TopSellingProductDto>>> GetTopSelling(CancellationToken ct, [FromQuery] string period = "1Y")
    {
        return Ok(await _mediator.Send(new GetTopSellingProductsQuery { Period = period }, ct));
    }

    [HttpGet("recent-sales")]
    public async Task<ActionResult<List<RecentSaleDto>>> GetRecentSales(CancellationToken ct, [FromQuery] string period = "1Y")
    {
        return Ok(await _mediator.Send(new GetRecentSalesQuery { Period = period }, ct));
    }
}
