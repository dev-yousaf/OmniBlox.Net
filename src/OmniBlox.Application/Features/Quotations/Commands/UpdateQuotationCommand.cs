using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Quotations.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Quotations.Commands;

public record UpdateQuotationCommand : IRequest<QuotationDetailDto>
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public DateTime QuoteDate { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public string Status { get; init; } = "DRAFT";
    public string? Notes { get; init; }
    public List<CreateQuotationItemCommand> Items { get; init; } = [];
}

public class UpdateQuotationCommandHandler : IRequestHandler<UpdateQuotationCommand, QuotationDetailDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateQuotationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<QuotationDetailDto> Handle(UpdateQuotationCommand request, CancellationToken ct)
    {
        var quotation = await _context.Quotations
            .Include(q => q.Items)
            .FirstOrDefaultAsync(q => q.Id == request.Id, ct);

        if (quotation is null)
            throw new NotFoundException(nameof(Quotation), request.Id);

        var customerExists = await _context.Customers
            .AnyAsync(c => c.Id == request.CustomerId, ct);

        if (!customerExists)
            throw new NotFoundException(nameof(Customer), request.CustomerId);

        quotation.CustomerId = request.CustomerId;
        quotation.QuoteDate = request.QuoteDate;
        quotation.ExpiryDate = request.ExpiryDate;
        quotation.Status = request.Status;
        quotation.Notes = request.Notes;

        _context.QuotationItems.RemoveRange(quotation.Items);

        decimal totalAmount = 0;
        var newItems = new List<QuotationItem>();

        foreach (var item in request.Items)
        {
            var productExists = await _context.Products
                .AnyAsync(p => p.Id == item.ProductId, ct);

            if (!productExists)
                throw new NotFoundException(nameof(Product), item.ProductId);

            var quotationItem = new QuotationItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
            };

            newItems.Add(quotationItem);
            totalAmount += item.Quantity * item.UnitPrice;
        }

        quotation.TotalAmount = totalAmount;
        quotation.Items = newItems;
        quotation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        quotation = await _context.Quotations
            .Include(q => q.Customer)
            .Include(q => q.Items).ThenInclude(i => i.Product)
            .FirstAsync(q => q.Id == quotation.Id, ct);

        return QuotationDetailDto.FromEntity(quotation);
    }
}

public class UpdateQuotationCommandValidator : AbstractValidator<UpdateQuotationCommand>
{
    public UpdateQuotationCommandValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty();

        RuleFor(v => v.CustomerId)
            .NotEmpty();

        RuleFor(v => v.QuoteDate)
            .NotEmpty();

        RuleFor(v => v.Items)
            .NotEmpty().WithMessage("At least one item is required.");

        RuleForEach(v => v.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}
