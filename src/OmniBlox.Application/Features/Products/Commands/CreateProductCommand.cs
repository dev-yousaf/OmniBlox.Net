using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Products.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Products.Commands;

public record CreateProductCommand : IRequest<ProductDto>
{
    public string Name { get; init; } = string.Empty;
    public string SKU { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Type { get; init; } = "STANDARD";
    public string Category { get; init; } = string.Empty;
    public string? SubCategory { get; init; }
    public string? Brand { get; init; }
    public string Unit { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public decimal SalePrice { get; init; }
    public decimal CostPrice { get; init; }
    public int Stock { get; init; }
    public int ReorderLevel { get; init; }
    public string Status { get; init; } = "ACTIVE";
    public string? BarcodeSymbology { get; init; }
    public decimal? TaxRate { get; init; }
    public int? AlertQuantity { get; init; }
    public string? ItemCode { get; init; }
    public string? Manufacturer { get; init; }
    public string? Warranty { get; init; }
    public DateTime? ManufacturedDate { get; init; }
    public DateTime? ExpiryDate { get; init; }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IApplicationDbContext _context;

    public CreateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var skuExists = await _context.Products
            .AnyAsync(p => p.SKU == request.SKU, ct);

        if (skuExists)
            throw new ConflictException($"Product with SKU '{request.SKU}' already exists.");

        var product = new Product
        {
            Name = request.Name,
            SKU = request.SKU,
            Description = request.Description,
            Type = Enum.TryParse<ProductType>(request.Type, true, out var type) ? type : ProductType.STANDARD,
            Category = request.Category,
            SubCategory = request.SubCategory,
            Brand = request.Brand,
            Unit = request.Unit,
            ImageUrl = request.ImageUrl,
            SalePrice = request.SalePrice,
            CostPrice = request.CostPrice,
            Stock = request.Stock,
            ReorderLevel = request.ReorderLevel,
            Status = Enum.TryParse<ProductStatus>(request.Status, true, out var status) ? status : ProductStatus.ACTIVE,
            BarcodeSymbology = request.BarcodeSymbology,
            TaxRate = request.TaxRate,
            AlertQuantity = request.AlertQuantity,
            ItemCode = request.ItemCode,
            Manufacturer = request.Manufacturer,
            Warranty = request.Warranty,
            ManufacturedDate = request.ManufacturedDate,
            ExpiryDate = request.ExpiryDate,
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync(ct);

        return ProductDto.FromEntity(product);
    }
}

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200);

        RuleFor(v => v.SKU)
            .NotEmpty().WithMessage("SKU is required.")
            .MaximumLength(100);

        RuleFor(v => v.Category)
            .NotEmpty().WithMessage("Category is required.")
            .MaximumLength(100);

        RuleFor(v => v.Unit)
            .NotEmpty().WithMessage("Unit is required.")
            .MaximumLength(50);

        RuleFor(v => v.SalePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Sale price must be non-negative.");

        RuleFor(v => v.CostPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Cost price must be non-negative.");

        RuleFor(v => v.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity must be non-negative.");

        RuleFor(v => v.ReorderLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Reorder level must be non-negative.");

        RuleFor(v => v.TaxRate)
            .InclusiveBetween(0, 100).When(v => v.TaxRate.HasValue)
            .WithMessage("Tax rate must be between 0 and 100.");
    }
}
