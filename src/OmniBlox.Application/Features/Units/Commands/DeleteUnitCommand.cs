using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Units.Commands;

public record DeleteUnitCommand : IRequest
{
    public Guid Id { get; init; }
}

public class DeleteUnitCommandHandler : IRequestHandler<DeleteUnitCommand>
{
    private readonly IApplicationDbContext _context;
    public DeleteUnitCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(DeleteUnitCommand request, CancellationToken ct)
    {
        var entity = await _context.Units.AsTracking().FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null) throw new NotFoundException(nameof(Domain.Entities.Unit), request.Id);
        _context.Units.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }
}
