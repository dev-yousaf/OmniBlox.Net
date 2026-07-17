using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.SalesReturns.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.SalesReturns.Queries;

public record GetSalesReturnQuery : IRequest<SalesReturnDetailDto>
{
    public Guid Id { get; init; }
}

public class GetSalesReturnQueryHandler : IRequestHandler<GetSalesReturnQuery, SalesReturnDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetSalesReturnQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SalesReturnDetailDto> Handle(GetSalesReturnQuery request, CancellationToken ct)
    {
        var salesReturn = await _context.SalesReturns
            .Include(r => r.Warehouse)
            .Include(r => r.Sale)
            .Include(r => r.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == _currentUser.CompanyId, ct);

        if (salesReturn is null)
            throw new NotFoundException(nameof(SalesReturn), request.Id);

        return SalesReturnDetailDto.FromEntity(salesReturn);
    }
}
