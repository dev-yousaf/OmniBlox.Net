using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.VariantAttributes.DTOs;

namespace OmniBlox.Application.Features.VariantAttributes.Queries;

public record GetVariantAttributesQuery : IRequest<List<VariantAttributeDto>>;

public class GetVariantAttributesQueryHandler : IRequestHandler<GetVariantAttributesQuery, List<VariantAttributeDto>>
{
    private readonly IApplicationDbContext _context;
    public GetVariantAttributesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<VariantAttributeDto>> Handle(GetVariantAttributesQuery request, CancellationToken ct)
    {
        var items = await _context.VariantAttributes.OrderBy(x => x.Name).ToListAsync(ct);
        return items.Select(VariantAttributeDto.FromEntity).ToList();
    }
}
