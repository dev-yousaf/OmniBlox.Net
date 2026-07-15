using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;

namespace OmniBlox.Application.Features.Products.Queries;

public record ExportProductsQuery : IRequest<string>;

public class ExportProductsQueryHandler : IRequestHandler<ExportProductsQuery, string>
{
    private readonly IApplicationDbContext _context;
    public ExportProductsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<string> Handle(ExportProductsQuery request, CancellationToken ct)
    {
        var products = await _context.Products
            .OrderBy(p => p.Name)
            .ToListAsync(ct);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Name,SKU,Category,Brand,Unit,SalePrice,CostPrice,Stock,Description");
        foreach (var p in products)
        {
            sb.AppendLine($"\"{p.Name}\",\"{p.SKU}\",\"{p.Category}\",\"{p.Brand}\",\"{p.Unit}\",{p.SalePrice},{p.CostPrice},{p.Stock},\"{p.Description}\"");
        }
        return sb.ToString();
    }
}
