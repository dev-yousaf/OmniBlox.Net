using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.SubCategories.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.SubCategories.Queries;

public record GetSubCategoryByIdQuery : IRequest<SubCategoryDto>
{
    public Guid Id { get; init; }
}

public class GetSubCategoryByIdQueryHandler : IRequestHandler<GetSubCategoryByIdQuery, SubCategoryDto>
{
    private readonly IApplicationDbContext _context;
    public GetSubCategoryByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<SubCategoryDto> Handle(GetSubCategoryByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.SubCategories.Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null) throw new NotFoundException(nameof(SubCategory), request.Id);
        return SubCategoryDto.FromEntity(entity);
    }
}
