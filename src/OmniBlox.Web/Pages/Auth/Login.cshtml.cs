using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OmniBlox.Application.Features.Auth.Commands;
using System.Security.Claims;

namespace OmniBlox.Web.Pages;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly IMediator _mediator;

    public LoginModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var result = await _mediator.Send(new LoginCommand
            {
                Email = Email,
                Password = Password,
            });

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, result.User.Id.ToString()),
                new Claim(ClaimTypes.Email, result.User.Email),
                new Claim(ClaimTypes.Role, result.User.Role),
                new Claim(ClaimTypes.Name, result.User.Name),
                new Claim("companyId", result.Company.Id.ToString()),
                new Claim("companyName", result.Company.Name),
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTime.UtcNow.AddDays(7) });

            Response.Cookies.Append("access_token", result.Token, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                MaxAge = TimeSpan.FromDays(7),
            });

            return RedirectToPage("/Index");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
    }

    public async Task<IActionResult> OnPostLogoutAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        Response.Cookies.Delete("access_token");
        return RedirectToPage("/Auth/Login");
    }
}
