using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Quotations.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;
using OmniBlox.Shared.Extensions;

namespace OmniBlox.Application.Features.Quotations.Commands;

public record CreateQuotationCommand : IRequest<QuotationDetailDto>
{
    public Guid CustomerId { get; init; }
    public DateTime QuoteDate { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public string Status { get; init; } = "DRAFT";
    public string? Notes { get; init; }
    public List<CreateQuotationItemCommand> Items { get; init; } = [];
}

public record CreateQuotationItemCommand
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}

public class CreateQuotationCommandHandler : IRequestHandler<CreateQuotationCommand, QuotationDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateQuotationCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<QuotationDetailDto> Handle(CreateQuotationCommand request, CancellationToken ct)
    {
        var customer = await _context.Customers
            .AnyAsync(c => c.Id == request.CustomerId, ct);

        if (!customer)
            throw new NotFoundException(nameof(Customer), request.CustomerId);

        var reference = "QT-" +
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + "-" +
            Random.Shared.Next(1000, 9999);

        var items = new List<QuotationItem>();
        decimal totalAmount = 0;

        foreach (var item in request.Items)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == item.ProductId, ct);

            if (product is null)
                throw new NotFoundException(nameof(Product), item.ProductId);

            var quotationItem = new QuotationItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
            };

            items.Add(quotationItem);
            totalAmount += item.Quantity * item.UnitPrice;
        }

        var quotation = new Quotation
        {
            ReferenceNumber = reference,
            CustomerId = request.CustomerId,
            UserId = _currentUser.UserId,
            QuoteDate = request.QuoteDate.AsUtc(),
            ExpiryDate = request.ExpiryDate.AsUtcOrNull(),
            Status = request.Status,
            Notes = request.Notes,
            TotalAmount = totalAmount,
            Items = items,
        };

        _context.Quotations.Add(quotation);
        await _context.SaveChangesAsync(ct);

        quotation = await _context.Quotations
            .Include(q => q.Customer)
            .Include(q => q.Items).ThenInclude(i => i.Product)
            .FirstAsync(q => q.Id == quotation.Id, ct);

        return QuotationDetailDto.FromEntity(quotation);
    }
}

public class CreateQuotationCommandValidator : AbstractValidator<CreateQuotationCommand>
{
    public CreateQuotationCommandValidator()
    {
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
