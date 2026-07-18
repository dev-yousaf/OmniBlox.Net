using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Warehouses.Commands;

public record DeleteWarehouseCommand : IRequest
{
    public Guid Id { get; init; }
}

public class DeleteWarehouseCommandHandler : IRequestHandler<DeleteWarehouseCommand>
{
    private readonly IApplicationDbContext _context;
    public DeleteWarehouseCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(DeleteWarehouseCommand request, CancellationToken ct)
    {
        var entity = await _context.Warehouses
            .Include(w => w.Inventories)
            .AsTracking().FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null) throw new NotFoundException(nameof(Warehouse), request.Id);
        if (entity.Inventories.Count > 0)
            throw new ConflictException("Cannot delete warehouse with existing inventory. Transfer or remove stock first.");

        _context.Warehouses.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }
}
