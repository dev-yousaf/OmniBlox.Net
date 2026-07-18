using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Billers.DTOs;

namespace OmniBlox.Application.Features.Billers.Queries;

public record GetBillersQuery : IRequest<List<BillerDto>>;

public class GetBillersQueryHandler : IRequestHandler<GetBillersQuery, List<BillerDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetBillersQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<BillerDto>> Handle(GetBillersQuery request, CancellationToken ct)
    {
        var items = await _context.Billers.Where(e => e.CompanyId == _currentUser.CompanyId).OrderBy(x => x.Name).ToListAsync(ct);
        return items.Select(BillerDto.FromEntity).ToList();
    }
}
