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
    private readonly ICurrentUserService _currentUser;
    public GetSubCategoryByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SubCategoryDto> Handle(GetSubCategoryByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.SubCategories.Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == _currentUser.CompanyId, ct);
        if (entity is null) throw new NotFoundException(nameof(SubCategory), request.Id);
        return SubCategoryDto.FromEntity(entity);
    }
}
