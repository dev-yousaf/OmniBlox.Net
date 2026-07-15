using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Warranties.Commands;

public record DeleteWarrantyCommand : IRequest
{
    public Guid Id { get; init; }
}

public class DeleteWarrantyCommandHandler : IRequestHandler<DeleteWarrantyCommand>
{
    private readonly IApplicationDbContext _context;
    public DeleteWarrantyCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(DeleteWarrantyCommand request, CancellationToken ct)
    {
        var entity = await _context.Warranties.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null) throw new NotFoundException(nameof(Warranty), request.Id);
        _context.Warranties.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }
}
