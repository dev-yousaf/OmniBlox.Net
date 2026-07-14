using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OmniBlox.Application.Features.Products.DTOs;
using OmniBlox.Application.Features.Products.Queries;

namespace OmniBlox.Web.Pages;

[Authorize]
public class ProductsIndexModel : PageModel
{
    private readonly IMediator _mediator;

    public ProductsIndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public List<ProductDto> Products { get; set; } = [];
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int Total { get; set; }
    public string? Search { get; set; }

    public async Task OnGetAsync([FromQuery] int page = 1, [FromQuery] string? search = null)
    {
        CurrentPage = page;
        Search = search;

        var result = await _mediator.Send(new GetProductsQuery
        {
            Page = page,
            Limit = 20,
            Search = search,
        });

        Products = result.Items;
        Total = result.Total;
        TotalPages = (int)Math.Ceiling((double)result.Total / result.Limit);
    }
}
