using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.SubCategories.Commands;

public record DeleteSubCategoryCommand : IRequest
{
    public Guid Id { get; init; }
}

public class DeleteSubCategoryCommandHandler : IRequestHandler<DeleteSubCategoryCommand>
{
    private readonly IApplicationDbContext _context;
    public DeleteSubCategoryCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(DeleteSubCategoryCommand request, CancellationToken ct)
    {
        var entity = await _context.SubCategories.AsTracking().FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null) throw new NotFoundException(nameof(SubCategory), request.Id);
        _context.SubCategories.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }
}
