using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OmniBlox.Application.Features.Products.Commands;
using OmniBlox.Application.Features.Products.DTOs;
using OmniBlox.Application.Features.Products.Queries;

namespace OmniBlox.Web.Pages;

[Authorize]
public class EditProductModel : PageModel
{
    private readonly IMediator _mediator;

    public EditProductModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty]
    public Guid Id { get; set; }

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
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        try
        {
            var product = await _mediator.Send(new GetProductQuery { Id = id });
            Id = product.Id;
            Name = product.Name;
            SKU = product.SKU;
            Description = product.Description;
            Category = product.Category;
            Brand = product.Brand;
            Unit = product.Unit;
            Status = product.Status;
            SalePrice = product.SalePrice;
            CostPrice = product.CostPrice;
            StockQuantity = product.StockQuantity;
            ReorderLevel = product.ReorderLevel;
            TaxRate = product.TaxRate;
            AlertQuantity = product.AlertQuantity;
            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var command = new UpdateProductCommand
            {
                Id = Id,
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

            await _mediator.Send(command);
            SuccessMessage = "Product updated.";
            return RedirectToPage(new { id = Id });
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        try
        {
            await _mediator.Send(new DeleteProductCommand { Id = id });
            return RedirectToPage("/Products/Index");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
    }
}
