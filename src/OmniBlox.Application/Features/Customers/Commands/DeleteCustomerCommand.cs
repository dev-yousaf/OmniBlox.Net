using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Customers.Commands;

public record DeleteCustomerCommand : IRequest
{
    public Guid Id { get; init; }
}

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteCustomerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteCustomerCommand request, CancellationToken ct)
    {
        var customer = await _context.Customers
            .AsTracking().FirstOrDefaultAsync(c => c.Id == request.Id, ct);

        if (customer is null)
            throw new NotFoundException(nameof(Customer), request.Id);

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync(ct);
    }
}
