using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.VariantAttributes.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.VariantAttributes.Queries;

public record GetVariantAttributeByIdQuery : IRequest<VariantAttributeDto>
{
    public Guid Id { get; init; }
}

public class GetVariantAttributeByIdQueryHandler : IRequestHandler<GetVariantAttributeByIdQuery, VariantAttributeDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetVariantAttributeByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<VariantAttributeDto> Handle(GetVariantAttributeByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.VariantAttributes.FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == _currentUser.CompanyId, ct);
        if (entity is null) throw new NotFoundException(nameof(VariantAttribute), request.Id);
        return VariantAttributeDto.FromEntity(entity);
    }
}
