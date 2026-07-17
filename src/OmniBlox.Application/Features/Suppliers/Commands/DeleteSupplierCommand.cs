using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Suppliers.Commands;

public record DeleteSupplierCommand : IRequest
{
    public Guid Id { get; init; }
}

public class DeleteSupplierCommandHandler : IRequestHandler<DeleteSupplierCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteSupplierCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteSupplierCommand request, CancellationToken ct)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == request.Id, ct);

        if (supplier is null)
            throw new NotFoundException(nameof(Supplier), request.Id);

        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync(ct);
    }
}
