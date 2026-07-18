using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.ExpenseCategories.Commands;

public record DeleteExpenseCategoryCommand : IRequest
{
    public Guid Id { get; init; }
}

public class DeleteExpenseCategoryCommandHandler : IRequestHandler<DeleteExpenseCategoryCommand>
{
    private readonly IApplicationDbContext _context;
    public DeleteExpenseCategoryCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task Handle(DeleteExpenseCategoryCommand request, CancellationToken ct)
    {
        var entity = await _context.ExpenseCategories.AsTracking().FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null) throw new NotFoundException(nameof(ExpenseCategory), request.Id);
        _context.ExpenseCategories.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }
}
