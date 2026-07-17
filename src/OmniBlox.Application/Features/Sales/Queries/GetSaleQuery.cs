using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Sales.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Sales.Queries;

public record GetSaleQuery : IRequest<SaleDetailDto>
{
    public Guid Id { get; init; }
}

public class GetSaleQueryHandler : IRequestHandler<GetSaleQuery, SaleDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetSaleQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SaleDetailDto> Handle(GetSaleQuery request, CancellationToken ct)
    {
        var sale = await _context.Sales
            .Include(s => s.Customer)
            .Include(s => s.Warehouse)
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.CompanyId == _currentUser.CompanyId, ct);

        if (sale is null)
            throw new NotFoundException(nameof(Sale), request.Id);

        return SaleDetailDto.FromEntity(sale);
    }
}
