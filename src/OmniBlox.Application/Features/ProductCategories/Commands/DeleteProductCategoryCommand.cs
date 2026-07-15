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
    public DeleteProductCategoryCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<DeleteCategoryResponse> Handle(DeleteProductCategoryCommand request, CancellationToken ct)
    {
        var entity = await _context.ProductCategories.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null) throw new NotFoundException(nameof(ProductCategory), request.Id);

        var affectedProducts = await _context.Products
            .Where(p => p.Category == entity.Name)
            .Select(p => new AffectedProduct { Id = p.Id, Name = p.Name, Sku = p.SKU })
            .ToListAsync(ct);

        _context.ProductCategories.Remove(entity);
        await _context.SaveChangesAsync(ct);

        return new DeleteCategoryResponse
        {
            Message = affectedProducts.Count > 0
                ? $"Category deleted. {affectedProducts.Count} product(s) are no longer associated with this category."
                : "Category deleted successfully.",
            AffectedProducts = affectedProducts,
        };
    }
}
