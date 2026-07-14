using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OmniBlox.Application.Features.Products.Commands;

namespace OmniBlox.Web.Pages;

[Authorize]
public class CreateProductModel : PageModel
{
    private readonly IMediator _mediator;

    public CreateProductModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty]
    public string Name { get; set; } = string.Empty;

    [BindProperty]
    public string SKU { get; set; } = string.Empty;

    [BindProperty]
    public string? Description { get; set; }

    [BindProperty]
    public string Category { get; set; } = string.Empty;

    [BindProperty]
    public string? Brand { get; set; }

    [BindProperty]
    public string Unit { get; set; } = string.Empty;

    [BindProperty]
    public string Status { get; set; } = "ACTIVE";

    [BindProperty]
    public decimal SalePrice { get; set; }

    [BindProperty]
    public decimal CostPrice { get; set; }

    [BindProperty]
    public int StockQuantity { get; set; }

    [BindProperty]
    public int ReorderLevel { get; set; }

    [BindProperty]
    public decimal? TaxRate { get; set; }

    [BindProperty]
    public int? AlertQuantity { get; set; }

    public string? ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var command = new CreateProductCommand
            {
                Name = Name,
                SKU = SKU,
                Description = Description,
                Category = Category,
                Brand = Brand,
                Unit = Unit,
                Status = Status,
                SalePrice = SalePrice,
                CostPrice = CostPrice,
                StockQuantity = StockQuantity,
                ReorderLevel = ReorderLevel,
                TaxRate = TaxRate,
                AlertQuantity = AlertQuantity,
            };

            var result = await _mediator.Send(command);
            return RedirectToPage("/Products/Index");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
    }
}
