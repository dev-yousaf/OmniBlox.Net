namespace OmniBlox.Application.Features.Auth.DTOs;

public record AuthResponse
{
    public string Token { get; init; } = string.Empty;
    public UserDto User { get; init; } = null!;
    public CompanyDto Company { get; init; } = null!;
}

public record UserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public Guid CompanyId { get; init; }
    public CompanyDto? Company { get; init; }
}

public record CompanyDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string WorkspaceUrl { get; init; } = string.Empty;
    public string? Industry { get; init; }
    public string? Country { get; init; }
}
