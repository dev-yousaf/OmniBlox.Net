using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Products.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Products.Queries;

public record GetProductBySkuQuery : IRequest<ProductDto>
{
    public string Sku { get; init; } = string.Empty;
}

public class GetProductBySkuQueryHandler : IRequestHandler<GetProductBySkuQuery, ProductDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetProductBySkuQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ProductDto> Handle(GetProductBySkuQuery request, CancellationToken ct)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.SKU == request.Sku && p.CompanyId == _currentUser.CompanyId, ct);
        if (product is null) throw new NotFoundException(nameof(Product), request.Sku);
        return ProductDto.FromEntity(product);
    }
}
