using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Warranties.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Warranties.Queries;

public record GetWarrantyByIdQuery : IRequest<WarrantyDto>
{
    public Guid Id { get; init; }
}

public class GetWarrantyByIdQueryHandler : IRequestHandler<GetWarrantyByIdQuery, WarrantyDto>
{
    private readonly IApplicationDbContext _context;
    public GetWarrantyByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<WarrantyDto> Handle(GetWarrantyByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.Warranties.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null) throw new NotFoundException(nameof(Warranty), request.Id);
        return WarrantyDto.FromEntity(entity);
    }
}
