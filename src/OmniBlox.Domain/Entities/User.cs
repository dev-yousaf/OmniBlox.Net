using OmniBlox.Domain.Enums;

namespace OmniBlox.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.OBSERVER;
    public UserStatus Status { get; set; } = UserStatus.INVITED;

    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;
}
