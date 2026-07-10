using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniBlox.Application.Features.Auth.Commands;
using OmniBlox.Application.Features.Auth.DTOs;
using OmniBlox.Application.Features.Auth.Queries;

namespace OmniBlox.Api.Controllers;

[Route("auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("signup")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Signup(
        SignupRequest request,
        CancellationToken ct)
    {
        var command = new SignupCommand
        {
            Email = request.Email,
            Password = request.Password,
            Name = request.Name,
            CompanyName = request.CompanyName,
        };

        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request,
        CancellationToken ct)
    {
        var command = new LoginCommand
        {
            Email = request.Email,
            Password = request.Password,
        };

        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<AuthResponse>> GetMe(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCurrentUserQuery(), ct);
        return Ok(result);
    }
}
