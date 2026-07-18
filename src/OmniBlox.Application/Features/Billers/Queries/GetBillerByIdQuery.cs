using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Billers.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Billers.Queries;

public record GetBillerByIdQuery : IRequest<BillerDto>
{
    public Guid Id { get; init; }
}

public class GetBillerByIdQueryHandler : IRequestHandler<GetBillerByIdQuery, BillerDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetBillerByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<BillerDto> Handle(GetBillerByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.Billers.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == _currentUser.CompanyId, ct);
        if (entity is null) throw new NotFoundException(nameof(Biller), request.Id);
        return BillerDto.FromEntity(entity);
    }
}
