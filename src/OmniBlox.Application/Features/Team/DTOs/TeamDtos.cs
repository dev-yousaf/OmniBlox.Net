namespace OmniBlox.Application.Features.Team.DTOs;

public record TeamUserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public string? InviteToken { get; init; }

    public static TeamUserDto FromEntity(Domain.Entities.User u, string? inviteToken = null) => new()
    {
        Id = u.Id,
        Email = u.Email,
        Name = u.Name,
        Role = u.Role.ToString(),
        Status = u.Status.ToString(),
        CreatedAt = u.CreatedAt,
        LastLoginAt = u.LastLoginAt,
        InviteToken = inviteToken,
    };
}

public record TeamListResponse
{
    public List<TeamUserDto> Users { get; init; } = [];
    public int Total { get; init; }
    public int Pages { get; init; }
}

public record TeamStatsDto
{
    public int TotalUsers { get; init; }
    public int AdminCount { get; init; }
    public int ManagerCount { get; init; }
    public int StaffCount { get; init; }
    public int ActiveUsers { get; init; }
    public int InactiveUsers { get; init; }
    public int InvitedUsers { get; init; }
}

public record CreateUserRequest
{
    public string Email { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Role { get; init; }
}

public record UpdateUserRequest
{
    public string? Email { get; init; }
    public string? Name { get; init; }
    public string? Role { get; init; }
}

public record ChangePasswordRequest
{
    public string CurrentPassword { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}

public record AcceptInvitationRequest
{
    public string Token { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
