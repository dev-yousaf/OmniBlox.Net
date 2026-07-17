using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.SalesReturns.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.SalesReturns.Commands;

public record CreateSalesReturnCommand : IRequest<SalesReturnDetailDto>
{
    public Guid WarehouseId { get; init; }
    public Guid? SaleId { get; init; }
    public string? Reason { get; init; }
    public DateTime ReturnDate { get; init; }
    public List<CreateSalesReturnItem> Items { get; init; } = new();
}

public class CreateSalesReturnCommandHandler : IRequestHandler<CreateSalesReturnCommand, SalesReturnDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public CreateSalesReturnCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SalesReturnDetailDto> Handle(CreateSalesReturnCommand request, CancellationToken ct)
    {
        var warehouse = await _context.Warehouses.FirstOrDefaultAsync(x => x.Id == request.WarehouseId, ct);
        if (warehouse is null)
            throw new NotFoundException(nameof(Warehouse), request.WarehouseId);

        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var products = await _context.Products.Where(p => productIds.Contains(p.Id)).ToListAsync(ct);
        if (products.Count != productIds.Count)
        {
            var missing = productIds.Except(products.Select(p => p.Id)).ToList();
            throw new NotFoundException(nameof(Product), string.Join(",", missing));
        }

        Sale? sale = null;
        if (request.SaleId.HasValue)
        {
            sale = await _context.Sales
                .Include(s => s.Items)
                .FirstOrDefaultAsync(x => x.Id == request.SaleId.Value, ct);

            if (sale is null)
                throw new NotFoundException(nameof(Sale), request.SaleId.Value);

            foreach (var item in request.Items)
            {
                if (item.SaleItemId.HasValue)
                {
                    var saleItem = sale.Items.FirstOrDefault(si => si.Id == item.SaleItemId.Value);
                    if (saleItem is null)
                        throw new NotFoundException(nameof(SaleItem), item.SaleItemId.Value);

                    if (saleItem.ReturnedQuantity + item.Quantity > saleItem.Quantity)
                        throw new ConflictException($"SaleItem {saleItem.Id} would exceed available quantity for return.");
                }
            }
        }

        var referenceNumber = $"SRTN-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}-{Random.Shared.Next(1000, 9999)}";
        var totalAmount = request.Items.Sum(i => i.Quantity * i.UnitPrice);

        var salesReturn = new SalesReturn
        {
            ReferenceNumber = referenceNumber,
            WarehouseId = request.WarehouseId,
            UserId = _currentUser.UserId,
            SaleId = request.SaleId,
            Reason = request.Reason,
            ReturnDate = request.ReturnDate == default ? DateTime.UtcNow : request.ReturnDate,
            Status = "PENDING",
            TotalAmount = totalAmount,
        };

        salesReturn.Items = request.Items.Select(i => new SalesReturnItem
        {
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            SaleItemId = i.SaleItemId,
        }).ToList();

        _context.SalesReturns.Add(salesReturn);
        await _context.SaveChangesAsync(ct);

        var result = await _context.SalesReturns
            .Include(r => r.Warehouse)
            .Include(r => r.Sale)
            .Include(r => r.Items)
                .ThenInclude(i => i.Product)
            .FirstAsync(x => x.Id == salesReturn.Id, ct);

        return SalesReturnDetailDto.FromEntity(result);
    }
}

public class CreateSalesReturnCommandValidator : AbstractValidator<CreateSalesReturnCommand>
{
    public CreateSalesReturnCommandValidator()
    {
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.ReturnDate).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}
