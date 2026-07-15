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
    public GetBrandByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<BrandDto> Handle(GetBrandByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.Brands.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null) throw new NotFoundException(nameof(Brand), request.Id);
        return BrandDto.FromEntity(entity);
    }
}
