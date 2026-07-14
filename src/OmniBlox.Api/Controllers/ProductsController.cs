using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniBlox.Application.Features.Products.Commands;
using OmniBlox.Application.Features.Products.DTOs;
using OmniBlox.Application.Features.Products.Queries;

namespace OmniBlox.Api.Controllers;

[Route("products")]
[Authorize]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request, CancellationToken ct)
    {
        var command = new CreateProductCommand
        {
            Name = request.Name,
            SKU = request.SKU,
            Description = request.Description,
            Type = request.Type ?? "STANDARD",
            Category = request.Category,
            Brand = request.Brand,
            Unit = request.Unit,
            ImageUrl = request.ImageUrl,
            SalePrice = request.SalePrice,
            CostPrice = request.CostPrice,
            StockQuantity = request.StockQuantity,
            ReorderLevel = request.ReorderLevel,
            Status = request.Status ?? "ACTIVE",
            BarcodeSymbology = request.BarcodeSymbology,
            TaxRate = request.TaxRate,
            AlertQuantity = request.AlertQuantity,
            ItemCode = request.ItemCode,
            Manufacturer = request.Manufacturer,
            Warranty = request.Warranty,
            ManufacturedDate = request.ManufacturedDate,
            ExpiryDate = request.ExpiryDate,
        };

        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductQuery { Id = id }, ct);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<ProductListResponse>> GetAll(
        CancellationToken ct,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        [FromQuery] string? status = null)
    {
        var result = await _mediator.Send(new GetProductsQuery
        {
            Page = page,
            Limit = limit,
            Search = search,
            Category = category,
            Status = status,
        }, ct);

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDto>> Update(Guid id, UpdateProductRequest request, CancellationToken ct)
    {
        var command = new UpdateProductCommand
        {
            Id = id,
            Name = request.Name,
            SKU = request.SKU,
            Description = request.Description,
            Type = request.Type,
            Category = request.Category,
            Brand = request.Brand,
            Unit = request.Unit,
            ImageUrl = request.ImageUrl,
            SalePrice = request.SalePrice,
            CostPrice = request.CostPrice,
            StockQuantity = request.StockQuantity,
            ReorderLevel = request.ReorderLevel,
            Status = request.Status,
            BarcodeSymbology = request.BarcodeSymbology,
            TaxRate = request.TaxRate,
            AlertQuantity = request.AlertQuantity,
            ItemCode = request.ItemCode,
            Manufacturer = request.Manufacturer,
            Warranty = request.Warranty,
            ManufacturedDate = request.ManufacturedDate,
            ExpiryDate = request.ExpiryDate,
        };

        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteProductCommand { Id = id }, ct);
        return NoContent();
    }
}
