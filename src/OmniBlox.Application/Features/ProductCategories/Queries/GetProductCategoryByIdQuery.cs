using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.ProductCategories.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.ProductCategories.Queries;

public record GetProductCategoryByIdQuery : IRequest<ProductCategoryDto>
{
    public Guid Id { get; init; }
}

public class GetProductCategoryByIdQueryHandler : IRequestHandler<GetProductCategoryByIdQuery, ProductCategoryDto>
{
    private readonly IApplicationDbContext _context;
    public GetProductCategoryByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<ProductCategoryDto> Handle(GetProductCategoryByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.ProductCategories.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null) throw new NotFoundException(nameof(ProductCategory), request.Id);
        return ProductCategoryDto.FromEntity(entity);
    }
}
