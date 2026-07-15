using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Auth.DTOs;

namespace OmniBlox.Application.Features.Auth.Queries;

public record ValidateTokenQuery : IRequest<ValidateTokenResult>;

public record ValidateTokenResult
{
    public bool Valid { get; init; }
    public UserDto User { get; init; } = null!;
}

public class ValidateTokenQueryHandler : IRequestHandler<ValidateTokenQuery, ValidateTokenResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ValidateTokenQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ValidateTokenResult> Handle(ValidateTokenQuery request, CancellationToken ct)
    {
        var user = await _context.Users
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, ct);

        if (user is null)
        {
            return new ValidateTokenResult { Valid = false, User = null! };
        }

        return new ValidateTokenResult
        {
            Valid = true,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Role = user.Role.ToString(),
                CompanyId = user.CompanyId,
                Company = new CompanyDto
                {
                    Id = user.Company.Id,
                    Name = user.Company.Name,
                    WorkspaceUrl = user.Company.WorkspaceUrl,
                    Industry = user.Company.Industry,
                    OtherIndustry = user.Company.OtherIndustry,
                    Country = user.Company.Country,
                },
            },
        };
    }
}
