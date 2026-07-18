using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Billers.Commands;

public record DeleteBillerCommand : IRequest
{
    public Guid Id { get; init; }
}

public class DeleteBillerCommandHandler : IRequestHandler<DeleteBillerCommand>
{
    private readonly IApplicationDbContext _context;
    public DeleteBillerCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(DeleteBillerCommand request, CancellationToken ct)
    {
        var entity = await _context.Billers.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null) throw new NotFoundException(nameof(Biller), request.Id);
        _context.Billers.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }
}
