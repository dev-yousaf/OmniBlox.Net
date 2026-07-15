using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Products.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Products.Commands;

public record UpdateStockCommand : IRequest<ProductDto>
{
    public Guid Id { get; init; }
    public int Quantity { get; init; }
    public string Operation { get; init; } = "add";
}

public class UpdateStockCommandHandler : IRequestHandler<UpdateStockCommand, ProductDto>
{
    private readonly IApplicationDbContext _context;
    public UpdateStockCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<ProductDto> Handle(UpdateStockCommand request, CancellationToken ct)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.Id, ct);
        if (product is null) throw new NotFoundException(nameof(Product), request.Id);

        product.Stock = request.Operation.ToLower() switch
        {
            "add" => product.Stock + request.Quantity,
            "subtract" => product.Stock - request.Quantity,
            _ => throw new ArgumentException("Operation must be 'add' or 'subtract'"),
        };

        if (product.Stock < 0) product.Stock = 0;
        product.UpdatedAt = DateTime.UtcNow;

        _context.StockLedgerEntries.Add(new StockLedgerEntry
        {
            ProductId = product.Id,
            Quantity = request.Operation.ToLower() == "add" ? request.Quantity : -request.Quantity,
            Balance = product.Stock,
            Type = request.Operation.ToUpper(),
            Reference = $"Stock {request.Operation}",
        });

        await _context.SaveChangesAsync(ct);
        return ProductDto.FromEntity(product);
    }
}
