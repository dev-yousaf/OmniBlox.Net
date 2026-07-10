using OmniBlox.Domain.Entities;

namespace OmniBlox.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user, Company company);
}
