using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OmniBlox.Application.Features.Products.DTOs;
using OmniBlox.Application.Features.Products.Queries;

namespace OmniBlox.Web.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IMediator _mediator;

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public List<ProductDto> RecentProducts { get; set; } = [];
    public ProductStats Stats { get; set; } = new();

    public async Task OnGetAsync()
    {
        var result = await _mediator.Send(new GetProductsQuery
        {
            Page = 1,
            Limit = 5,
        });

        RecentProducts = result.Items;
        Stats = new ProductStats
        {
            TotalProducts = result.Total,
            LowStockCount = result.Items.Count(p => p.StockQuantity <= p.ReorderLevel),
            TotalValue = result.Items.Sum(p => p.StockQuantity * p.CostPrice),
        };
    }
}

public record ProductStats
{
    public int TotalProducts { get; init; }
    public int LowStockCount { get; init; }
    public decimal TotalValue { get; init; }
}
