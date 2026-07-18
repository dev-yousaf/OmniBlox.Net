using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Expenses.Commands;

public record DeleteExpenseCommand : IRequest
{
    public Guid Id { get; init; }
}

public class DeleteExpenseCommandHandler : IRequestHandler<DeleteExpenseCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteExpenseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteExpenseCommand request, CancellationToken ct)
    {
        var entity = await _context.Expenses
            .Include(e => e.Attachments)
            .FirstOrDefaultAsync(e => e.Id == request.Id, ct);
        if (entity is null) throw new NotFoundException(nameof(Expense), request.Id);

        _context.Expenses.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }
}
