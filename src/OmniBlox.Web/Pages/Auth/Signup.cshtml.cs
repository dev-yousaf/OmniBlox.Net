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
public class SignupModel : PageModel
{
    private readonly IMediator _mediator;

    public SignupModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty]
    public string Name { get; set; } = string.Empty;

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public string CompanyName { get; set; } = string.Empty;

    [BindProperty]
    public string WorkspaceUrl { get; set; } = string.Empty;

    [BindProperty]
    public string? Industry { get; set; }

    [BindProperty]
    public string? Country { get; set; }

    public string? ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var result = await _mediator.Send(new SignupCommand
            {
                Name = Name,
                Email = Email,
                Password = Password,
                CompanyName = CompanyName,
                WorkspaceUrl = WorkspaceUrl,
                Industry = Industry,
                Country = Country,
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
}
