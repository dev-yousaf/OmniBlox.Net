using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Customers.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Customers.Queries;

public record GetCustomerQuery : IRequest<CustomerDto>
{
    public Guid Id { get; init; }
}

public class GetCustomerQueryHandler : IRequestHandler<GetCustomerQuery, CustomerDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetCustomerQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<CustomerDto> Handle(GetCustomerQuery request, CancellationToken ct)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == request.Id && c.CompanyId == _currentUser.CompanyId, ct);

        if (customer is null)
            throw new NotFoundException(nameof(Customer), request.Id);

        return CustomerDto.FromEntity(customer);
    }
}
