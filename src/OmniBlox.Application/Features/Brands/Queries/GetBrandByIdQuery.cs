using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Brands.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Brands.Queries;

public record GetBrandByIdQuery : IRequest<BrandDto>
{
    public Guid Id { get; init; }
}

public class GetBrandByIdQueryHandler : IRequestHandler<GetBrandByIdQuery, BrandDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetBrandByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<BrandDto> Handle(GetBrandByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.Brands.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == _currentUser.CompanyId, ct);
        if (entity is null) throw new NotFoundException(nameof(Brand), request.Id);
        return BrandDto.FromEntity(entity);
    }
}
