using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Products.Commands;

public record BulkUpdatePriceCommand : IRequest
{
    public List<BulkPriceItem> Items { get; init; } = [];
}

public record BulkPriceItem
{
    public Guid Id { get; init; }
    public decimal SalePrice { get; init; }
    public decimal? CostPrice { get; init; }
}

public class BulkUpdatePriceCommandHandler : IRequestHandler<BulkUpdatePriceCommand>
{
    private readonly IApplicationDbContext _context;
    public BulkUpdatePriceCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(BulkUpdatePriceCommand request, CancellationToken ct)
    {
        foreach (var item in request.Items)
        {
            var product = await _context.Products.AsTracking().FirstOrDefaultAsync(p => p.Id == item.Id, ct);
            if (product is null) throw new NotFoundException(nameof(Domain.Entities.Product), item.Id);

            product.SalePrice = item.SalePrice;
            if (item.CostPrice.HasValue) product.CostPrice = item.CostPrice.Value;
            product.UpdatedAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync(ct);
    }
}
