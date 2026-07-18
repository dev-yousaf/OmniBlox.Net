using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.ProductCategories.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.ProductCategories.Commands;

public record DeleteProductCategoryCommand : IRequest<DeleteCategoryResponse>
{
    public Guid Id { get; init; }
}

public class DeleteProductCategoryCommandHandler : IRequestHandler<DeleteProductCategoryCommand, DeleteCategoryResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICrudService<ProductCategory, ProductCategoryDto> _crud;
    public DeleteProductCategoryCommandHandler(IApplicationDbContext context, ICrudService<ProductCategory, ProductCategoryDto> crud)
    {
        _context = context;
        _crud = crud;
    }

    public async Task<DeleteCategoryResponse> Handle(DeleteProductCategoryCommand request, CancellationToken ct)
    {
        var entity = await _context.ProductCategories.AsTracking().FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null) throw new NotFoundException(nameof(ProductCategory), request.Id);

        var affectedProducts = await _context.Products
            .Where(p => p.Category == entity.Name)
            .Select(p => new AffectedProduct { Id = p.Id, Name = p.Name, Sku = p.SKU })
            .AsTracking().ToListAsync(ct);

        await _crud.DeleteAsync(request.Id, ct);

        return new DeleteCategoryResponse
        {
            Message = affectedProducts.Count > 0
                ? $"Category deleted. {affectedProducts.Count} product(s) are no longer associated with this category."
                : "Category deleted successfully.",
            AffectedProducts = affectedProducts,
        };
    }
}
