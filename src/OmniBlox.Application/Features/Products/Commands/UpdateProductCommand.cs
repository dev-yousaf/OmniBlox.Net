using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Products.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Domain.Enums;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Products.Commands;

public record UpdateProductCommand : IRequest<ProductDto>
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? SKU { get; init; }
    public string? Description { get; init; }
    public string? Type { get; init; }
    public string? Category { get; init; }
    public string? SubCategory { get; init; }
    public string? Brand { get; init; }
    public string? Unit { get; init; }
    public string? ImageUrl { get; init; }
    public decimal? SalePrice { get; init; }
    public decimal? CostPrice { get; init; }
    public int? ReorderLevel { get; init; }
    public string? Status { get; init; }
    public string? BarcodeSymbology { get; init; }
    public decimal? TaxRate { get; init; }
    public int? AlertQuantity { get; init; }
    public string? ItemCode { get; init; }
    public string? Manufacturer { get; init; }
    public string? Warranty { get; init; }
    public DateTime? ManufacturedDate { get; init; }
    public DateTime? ExpiryDate { get; init; }

}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id, ct);

        if (product is null)
            throw new NotFoundException(nameof(Product), request.Id);

        if (request.SKU is not null && request.SKU != product.SKU)
        {
            var skuExists = await _context.Products
                .AnyAsync(p => p.SKU == request.SKU && p.Id != request.Id, ct);

            if (skuExists)
                throw new ConflictException($"Product with SKU '{request.SKU}' already exists.");
        }

        if (request.Name is not null) product.Name = request.Name;
        if (request.SKU is not null) product.SKU = request.SKU;
        if (request.Description is not null) product.Description = request.Description;
        if (request.Type is not null && Enum.TryParse<ProductType>(request.Type, true, out var type))
            product.Type = type;
        if (request.Category is not null) product.Category = request.Category;
        if (request.SubCategory is not null) product.SubCategory = request.SubCategory;
        if (request.Brand is not null) product.Brand = request.Brand;
        if (request.Unit is not null) product.Unit = request.Unit;
        if (request.ImageUrl is not null) product.ImageUrl = request.ImageUrl;
        if (request.SalePrice.HasValue) product.SalePrice = request.SalePrice.Value;
        if (request.CostPrice.HasValue) product.CostPrice = request.CostPrice.Value;
        if (request.ReorderLevel.HasValue) product.ReorderLevel = request.ReorderLevel.Value;
        if (request.Status is not null && Enum.TryParse<ProductStatus>(request.Status, true, out var status))
            product.Status = status;
        if (request.BarcodeSymbology is not null) product.BarcodeSymbology = request.BarcodeSymbology;
        if (request.TaxRate.HasValue) product.TaxRate = request.TaxRate;
        if (request.AlertQuantity.HasValue) product.AlertQuantity = request.AlertQuantity;
        if (request.ItemCode is not null) product.ItemCode = request.ItemCode;
        if (request.Manufacturer is not null) product.Manufacturer = request.Manufacturer;
        if (request.Warranty is not null) product.Warranty = request.Warranty;
        if (request.ManufacturedDate is not null) product.ManufacturedDate = DateTime.SpecifyKind(request.ManufacturedDate.Value, DateTimeKind.Utc);
        if (request.ExpiryDate is not null) product.ExpiryDate = DateTime.SpecifyKind(request.ExpiryDate.Value, DateTimeKind.Utc);

        product.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return ProductDto.FromEntity(product);
    }
}

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty();

        RuleFor(v => v.Name)
            .MaximumLength(200).When(v => v.Name is not null);

        RuleFor(v => v.SKU)
            .MaximumLength(100).When(v => v.SKU is not null);

        RuleFor(v => v.Category)
            .MaximumLength(100).When(v => v.Category is not null);

        RuleFor(v => v.Unit)
            .MaximumLength(50).When(v => v.Unit is not null);

        RuleFor(v => v.SalePrice)
            .GreaterThanOrEqualTo(0).When(v => v.SalePrice.HasValue);

        RuleFor(v => v.CostPrice)
            .GreaterThanOrEqualTo(0).When(v => v.CostPrice.HasValue);

        RuleFor(v => v.ReorderLevel)
            .GreaterThanOrEqualTo(0).When(v => v.ReorderLevel.HasValue);

        RuleFor(v => v.TaxRate)
            .InclusiveBetween(0, 100).When(v => v.TaxRate.HasValue);
    }
}
