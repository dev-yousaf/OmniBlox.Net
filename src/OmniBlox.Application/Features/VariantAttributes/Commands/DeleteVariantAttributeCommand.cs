using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.VariantAttributes.Commands;

public record DeleteVariantAttributeCommand : IRequest
{
    public Guid Id { get; init; }
}

public class DeleteVariantAttributeCommandHandler : IRequestHandler<DeleteVariantAttributeCommand>
{
    private readonly IApplicationDbContext _context;
    public DeleteVariantAttributeCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(DeleteVariantAttributeCommand request, CancellationToken ct)
    {
        var entity = await _context.VariantAttributes.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null) throw new NotFoundException(nameof(VariantAttribute), request.Id);
        _context.VariantAttributes.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }
}
