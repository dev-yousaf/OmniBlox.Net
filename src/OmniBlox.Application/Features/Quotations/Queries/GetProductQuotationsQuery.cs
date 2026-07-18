using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;

namespace OmniBlox.Application.Features.Quotations.Queries;

public record GetProductQuotationsQuery : IRequest<List<ProductQuotationDto>>
{
    public Guid ProductId { get; init; }
}

public record ProductQuotationDto
{
    public Guid Id { get; init; }
    public string ReferenceNumber { get; init; } = string.Empty;
    public DateTime QuoteDate { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal TotalPrice { get; init; }
}

public class GetProductQuotationsQueryHandler : IRequestHandler<GetProductQuotationsQuery, List<ProductQuotationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetProductQuotationsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<ProductQuotationDto>> Handle(GetProductQuotationsQuery request, CancellationToken ct)
    {
        var items = await _context.QuotationItems
            .Include(qi => qi.Quotation).ThenInclude(q => q!.Customer)
            .Where(qi => qi.ProductId == request.ProductId && qi.Quotation!.CompanyId == _currentUser.CompanyId)
            .OrderByDescending(qi => qi.Quotation!.CreatedAt)
            .Take(20)
            .ToListAsync(ct);

        return items.Select(qi => new ProductQuotationDto
        {
            Id = qi.Quotation!.Id,
            ReferenceNumber = qi.Quotation.ReferenceNumber,
            QuoteDate = qi.Quotation.QuoteDate,
            CustomerName = qi.Quotation.Customer?.Name ?? string.Empty,
            Quantity = qi.Quantity,
            UnitPrice = qi.UnitPrice,
            TotalPrice = qi.Quantity * qi.UnitPrice,
        }).ToList();
    }
}
