using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmniBlox.Application.Features.Brands.DTOs;
using OmniBlox.Application.Features.Brands.Queries;
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
    public ProductsController(IMediator mediator) => _mediator = mediator;

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
            SubCategory = request.SubCategory,
            Brand = request.Brand,
            Unit = request.Unit,
            ImageUrl = request.ImageUrl,
            SalePrice = request.SalePrice,
            CostPrice = request.CostPrice,
            Stock = request.Stock,
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
        return Ok(await _mediator.Send(new GetProductQuery { Id = id }, ct));
    }

    [HttpGet("sku/{sku}")]
    public async Task<ActionResult<ProductDto>> GetBySku(string sku, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetProductBySkuQuery { Sku = sku }, ct));
    }

    [HttpGet]
    public async Task<ActionResult<ProductListResponse>> GetAll(
        CancellationToken ct,
        [FromQuery] int page = 1, [FromQuery] int limit = 20,
        [FromQuery] string? search = null, [FromQuery] string? category = null,
        [FromQuery] string? status = null)
    {
        return Ok(await _mediator.Send(new GetProductsQuery
        {
            Page = page, Limit = limit, Search = search,
            Category = category, Status = status,
        }, ct));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDto>> Update(Guid id, UpdateProductRequest request, CancellationToken ct)
    {
        var command = new UpdateProductCommand
        {
            Id = id, Name = request.Name, SKU = request.SKU,
            Description = request.Description, Type = request.Type,
            Category = request.Category, SubCategory = request.SubCategory,
            Brand = request.Brand, Unit = request.Unit, ImageUrl = request.ImageUrl,
            SalePrice = request.SalePrice, CostPrice = request.CostPrice,
            Stock = request.Stock, ReorderLevel = request.ReorderLevel,
            Status = request.Status, BarcodeSymbology = request.BarcodeSymbology,
            TaxRate = request.TaxRate, AlertQuantity = request.AlertQuantity,
            ItemCode = request.ItemCode, Manufacturer = request.Manufacturer,
            Warranty = request.Warranty, ManufacturedDate = request.ManufacturedDate,
            ExpiryDate = request.ExpiryDate,
        };
        return Ok(await _mediator.Send(command, ct));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteProductCommand { Id = id }, ct);
        return NoContent();
    }

    [HttpPut("{id:guid}/stock")]
    public async Task<ActionResult<ProductDto>> UpdateStock(Guid id, [FromBody] UpdateStockRequest request, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new UpdateStockCommand
        {
            Id = id, Quantity = request.Quantity, Operation = request.Operation,
        }, ct));
    }

    [HttpGet("categories")]
    public async Task<ActionResult<List<string>>> GetCategories(CancellationToken ct)
    {
        var items = await _mediator.Send(new GetProductsQuery { Limit = 10000 }, ct);
        var categories = items.Products.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();
        return Ok(categories);
    }

    [HttpGet("brands")]
    public async Task<ActionResult<List<string>>> GetBrands(CancellationToken ct)
    {
        var brands = await _mediator.Send(new GetBrandsQuery(), ct);
        return Ok(brands.Select(b => b.Name).ToList());
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<List<ProductDto>>> GetLowStock(CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetLowStockProductsQuery(), ct));
    }

    [HttpGet("low-stock/details")]
    public async Task<ActionResult<LowStockDetailsResponse>> GetLowStockDetails(
        CancellationToken ct, [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        return Ok(await _mediator.Send(new GetLowStockDetailsQuery { Page = page, Limit = limit }, ct));
    }

    [HttpGet("expired")]
    public async Task<ActionResult<ProductListResponse>> GetExpired(
        CancellationToken ct, [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        return Ok(await _mediator.Send(new GetExpiredProductsQuery { Page = page, Limit = limit }, ct));
    }

    [HttpGet("stats")]
    public async Task<ActionResult<ProductStatsResponse>> GetStats(CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetProductStatsQuery(), ct));
    }

    [HttpGet("{id:guid}/ledger")]
    public async Task<ActionResult<List<StockLedgerEntryDto>>> GetLedger(Guid id, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new GetStockLedgerQuery { ProductId = id }, ct));
    }

    [HttpPost("import")]
    public async Task<ActionResult<ImportProductsResponse>> Import([FromBody] List<ImportProductItem> items, CancellationToken ct)
    {
        return Ok(await _mediator.Send(new ImportProductsCommand { Items = items }, ct));
    }

    [HttpGet("export")]
    public async Task<ActionResult<string>> Export(CancellationToken ct)
    {
        var csv = await _mediator.Send(new ExportProductsQuery(), ct);
        return Ok(csv);
    }

    [HttpPut("bulk-update-price")]
    public async Task<ActionResult> BulkUpdatePrice([FromBody] List<BulkPriceItem> items, CancellationToken ct)
    {
        await _mediator.Send(new BulkUpdatePriceCommand { Items = items }, ct);
        return NoContent();
    }

    [HttpPost("adjustments")]
    public async Task<ActionResult<AdjustStockResponse>> AdjustStock([FromBody] AdjustStockCommand command, CancellationToken ct)
    {
        return Ok(await _mediator.Send(command, ct));
    }
}

public record UpdateStockRequest
{
    public int Quantity { get; init; }
    public string Operation { get; init; } = "add";
}
