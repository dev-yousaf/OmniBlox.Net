using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.SalesReturns.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.SalesReturns.Commands;

public record UpdateSalesReturnCommand : IRequest<SalesReturnDetailDto>
{
    public Guid Id { get; init; }
    public Guid WarehouseId { get; init; }
    public Guid? SaleId { get; init; }
    public string? Reason { get; init; }
    public DateTime ReturnDate { get; init; }
    public List<CreateSalesReturnItem> Items { get; init; } = new();
}

public class UpdateSalesReturnCommandHandler : IRequestHandler<UpdateSalesReturnCommand, SalesReturnDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public UpdateSalesReturnCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SalesReturnDetailDto> Handle(UpdateSalesReturnCommand request, CancellationToken ct)
    {
        var returnEntity = await _context.SalesReturns
            .Include(r => r.Items)
            .Include(r => r.Warehouse)
            .Include(r => r.Sale)
            .FirstOrDefaultAsync(r => r.Id == request.Id, ct);
        if (returnEntity is null) throw new NotFoundException(nameof(SalesReturn), request.Id);

        var warehouse = await _context.Warehouses.FirstOrDefaultAsync(x => x.Id == request.WarehouseId, ct);
        if (warehouse is null) throw new NotFoundException(nameof(Warehouse), request.WarehouseId);

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
            if (sale is null) throw new NotFoundException(nameof(Sale), request.SaleId.Value);

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

        returnEntity.WarehouseId = request.WarehouseId;
        returnEntity.SaleId = request.SaleId;
        returnEntity.Reason = request.Reason;
        returnEntity.ReturnDate = request.ReturnDate;

        _context.SalesReturnItems.RemoveRange(returnEntity.Items);

        var totalAmount = request.Items.Sum(i => i.Quantity * i.UnitPrice);
        returnEntity.TotalAmount = totalAmount;

        returnEntity.Items = request.Items.Select(i => new SalesReturnItem
        {
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            SaleItemId = i.SaleItemId,
        }).ToList();

        returnEntity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        var result = await _context.SalesReturns
            .Include(r => r.Warehouse)
            .Include(r => r.Sale)
            .Include(r => r.Items).ThenInclude(i => i.Product)
            .FirstAsync(x => x.Id == returnEntity.Id, ct);

        return SalesReturnDetailDto.FromEntity(result);
    }
}

public class UpdateSalesReturnCommandValidator : AbstractValidator<UpdateSalesReturnCommand>
{
    public UpdateSalesReturnCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
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
