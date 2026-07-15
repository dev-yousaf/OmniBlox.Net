using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;

namespace OmniBlox.Application.Features.Products.Commands;

public record ImportProductsCommand : IRequest<ImportProductsResponse>
{
    public List<ImportProductItem> Items { get; init; } = [];
}

public record ImportProductItem
{
    public string Name { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Category { get; init; } = string.Empty;
    public string? Brand { get; init; }
    public string Unit { get; init; } = string.Empty;
    public decimal SalePrice { get; init; }
    public decimal CostPrice { get; init; }
    public int Stock { get; init; }
}

public record ImportProductsResponse
{
    public int Imported { get; init; }
    public List<string> Errors { get; init; } = [];
}

public class ImportProductsCommandHandler : IRequestHandler<ImportProductsCommand, ImportProductsResponse>
{
    private readonly IApplicationDbContext _context;
    public ImportProductsCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<ImportProductsResponse> Handle(ImportProductsCommand request, CancellationToken ct)
    {
        var imported = 0;
        var errors = new List<string>();

        foreach (var item in request.Items)
        {
            try
            {
                var skuExists = await _context.Products.AnyAsync(p => p.SKU == item.Sku, ct);
                if (skuExists)
                {
                    errors.Add($"SKU '{item.Sku}' already exists. Skipped.");
                    continue;
                }

                _context.Products.Add(new Product
                {
                    Name = item.Name,
                    SKU = item.Sku,
                    Description = item.Description,
                    Category = item.Category,
                    Brand = item.Brand,
                    Unit = item.Unit,
                    SalePrice = item.SalePrice,
                    CostPrice = item.CostPrice,
                    Stock = item.Stock,
                    Status = ProductStatus.ACTIVE,
                    Type = ProductType.STANDARD,
                });
                imported++;
            }
            catch (Exception ex)
            {
                errors.Add($"Error importing '{item.Name}': {ex.Message}");
            }
        }

        await _context.SaveChangesAsync(ct);
        return new ImportProductsResponse { Imported = imported, Errors = errors };
    }
}
