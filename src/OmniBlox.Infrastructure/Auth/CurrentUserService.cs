using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using OmniBlox.Application.Common.Interfaces;

namespace OmniBlox.Infrastructure.Auth;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            return value is not null ? Guid.Parse(value)
                : throw new UnauthorizedAccessException("User not authenticated.");
        }
    }

    public Guid CompanyId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User
                .FindFirstValue("companyId");

            return value is not null ? Guid.Parse(value) : Guid.Empty;
        }
    }

    public string Role =>
        _httpContextAccessor.HttpContext?.User
            .FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    public string Email =>
        _httpContextAccessor.HttpContext?.User
            .FindFirstValue(ClaimTypes.Email) ?? string.Empty;
}
