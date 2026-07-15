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
            WorkspaceUrl = request.WorkspaceUrl,
            Industry = request.Industry,
            OtherIndustry = request.OtherIndustry,
            Country = request.Country,
        };

        var result = await _mediator.Send(command, ct);
        SetTokenCookie(result.Token);
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
        SetTokenCookie(result.Token);
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public ActionResult Logout()
    {
        Response.Cookies.Delete("access_token");
        return Ok(new { message = "Logged out." });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetMe(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCurrentUserQuery(), ct);
        return Ok(result);
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<ActionResult<UserDto>> UpdateProfile(
        UpdateProfileRequest request,
        CancellationToken ct)
    {
    var command = new UpdateProfileCommand
    {
        Name = request.Name,
        CompanyName = request.CompanyName,
        Industry = request.Industry,
        OtherIndustry = request.OtherIndustry,
        Country = request.Country,
    };

        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpPut("change-password")]
    [Authorize]
    public async Task<ActionResult> ChangePassword(
        ChangePasswordRequest request,
        CancellationToken ct)
    {
        var command = new ChangePasswordCommand
        {
            CurrentPassword = request.CurrentPassword,
            NewPassword = request.NewPassword,
        };

        await _mediator.Send(command, ct);
        return Ok(new { message = "Password changed." });
    }

    [HttpGet("validate")]
    [Authorize]
    public async Task<ActionResult<ValidateTokenResult>> ValidateToken(CancellationToken ct)
    {
        var result = await _mediator.Send(new ValidateTokenQuery(), ct);
        return Ok(result);
    }

    private void SetTokenCookie(string token)
    {
        Response.Cookies.Append("access_token", token, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = false,
            MaxAge = TimeSpan.FromDays(7),
        });
    }
}

public record UpdateProfileRequest
{
    public string? Name { get; init; }
    public string? CompanyName { get; init; }
    public string? Industry { get; init; }
    public string? OtherIndustry { get; init; }
    public string? Country { get; init; }
}

public record ChangePasswordRequest
{
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}
