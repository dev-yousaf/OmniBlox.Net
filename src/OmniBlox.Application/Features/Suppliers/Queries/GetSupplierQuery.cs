using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Suppliers.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Suppliers.Queries;

public record GetSupplierQuery : IRequest<SupplierDto>
{
    public Guid Id { get; init; }
}

public class GetSupplierQueryHandler : IRequestHandler<GetSupplierQuery, SupplierDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetSupplierQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<SupplierDto> Handle(GetSupplierQuery request, CancellationToken ct)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == request.Id && s.CompanyId == _currentUser.CompanyId, ct);

        if (supplier is null)
            throw new NotFoundException(nameof(Supplier), request.Id);

        return SupplierDto.FromEntity(supplier);
    }
}
