using System.Security.Claims;

namespace OmniBlox.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(value);
    }

    public static Guid GetCompanyId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue("companyId");
        return Guid.Parse(value);
    }

    public static string GetRole(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    }
}
