namespace OmniBlox.Application.Features.Auth.DTOs;

public record AuthResponse
{
    public string Token { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public Guid CompanyId { get; init; }
}
