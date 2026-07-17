using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Quotations.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Quotations.Queries;

public record GetQuotationQuery : IRequest<QuotationDetailDto>
{
    public Guid Id { get; init; }
}

public class GetQuotationQueryHandler : IRequestHandler<GetQuotationQuery, QuotationDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetQuotationQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<QuotationDetailDto> Handle(GetQuotationQuery request, CancellationToken ct)
    {
        var quotation = await _context.Quotations
            .Include(q => q.Customer)
            .Include(q => q.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(q => q.Id == request.Id && q.CompanyId == _currentUser.CompanyId, ct);

        if (quotation is null)
            throw new NotFoundException(nameof(Quotation), request.Id);

        return QuotationDetailDto.FromEntity(quotation);
    }
}
