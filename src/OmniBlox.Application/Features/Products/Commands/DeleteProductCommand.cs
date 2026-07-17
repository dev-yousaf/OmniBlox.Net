using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Products.Commands;

public record DeleteProductCommand : IRequest
{
    public Guid Id { get; init; }
}

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteProductCommand request, CancellationToken ct)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct);

        if (product is null)
            throw new NotFoundException(nameof(Product), request.Id);

        var stockMovements = await _context.StockMovements
            .Where(x => x.ProductId == request.Id)
            .ToListAsync(ct);
        _context.StockMovements.RemoveRange(stockMovements);

        var inventories = await _context.Inventories
            .Where(x => x.ProductId == request.Id)
            .ToListAsync(ct);
        _context.Inventories.RemoveRange(inventories);

        _context.Products.Remove(product);
        await _context.SaveChangesAsync(ct);
    }
}
