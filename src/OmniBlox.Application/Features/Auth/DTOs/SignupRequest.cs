namespace OmniBlox.Application.Features.Auth.DTOs;

public record SignupRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string CompanyName { get; init; } = string.Empty;
    public string WorkspaceUrl { get; init; } = string.Empty;
    public string? Industry { get; init; }
    public string? OtherIndustry { get; init; }
    public string? Country { get; init; }
}
