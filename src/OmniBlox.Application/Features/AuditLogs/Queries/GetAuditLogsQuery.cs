using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.AuditLogs.DTOs;

namespace OmniBlox.Application.Features.AuditLogs.Queries;

public record GetAuditLogsQuery : IRequest<AuditLogListResponse>
{
    public int Page { get; init; } = 1;
    public int Limit { get; init; } = 20;
    public Guid? UserId { get; init; }
}

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, AuditLogListResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAuditLogsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<AuditLogListResponse> Handle(GetAuditLogsQuery request, CancellationToken ct)
    {
        var query = _context.AuditLogs
            .Include(l => l.User)
            .Where(l => l.CompanyId == _currentUser.CompanyId)
            .AsQueryable();

        if (request.UserId.HasValue)
            query = query.Where(l => l.UserId == request.UserId.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((request.Page - 1) * request.Limit)
            .Take(request.Limit)
            .ToListAsync(ct);

        return new AuditLogListResponse
        {
            Logs = items.Select(AuditLogDto.FromEntity).ToList(),
            Total = total,
            Pages = (int)Math.Ceiling((double)total / request.Limit),
            Page = request.Page,
            Limit = request.Limit,
        };
    }
}
