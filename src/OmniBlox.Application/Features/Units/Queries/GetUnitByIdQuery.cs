using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Units.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Units.Queries;

public record GetUnitByIdQuery : IRequest<UnitDto>
{
    public Guid Id { get; init; }
}

public class GetUnitByIdQueryHandler : IRequestHandler<GetUnitByIdQuery, UnitDto>
{
    private readonly IApplicationDbContext _context;
    public GetUnitByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<UnitDto> Handle(GetUnitByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.Units.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null) throw new NotFoundException(nameof(Domain.Entities.Unit), request.Id);
        return UnitDto.FromEntity(entity);
    }
}
