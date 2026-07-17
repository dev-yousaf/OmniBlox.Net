using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Quotations.Commands;

public record DeleteQuotationCommand : IRequest
{
    public Guid Id { get; init; }
}

public class DeleteQuotationCommandHandler : IRequestHandler<DeleteQuotationCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteQuotationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteQuotationCommand request, CancellationToken ct)
    {
        var quotation = await _context.Quotations
            .Include(q => q.Items)
            .FirstOrDefaultAsync(q => q.Id == request.Id, ct);

        if (quotation is null)
            throw new NotFoundException(nameof(Quotation), request.Id);

        _context.QuotationItems.RemoveRange(quotation.Items);
        _context.Quotations.Remove(quotation);
        await _context.SaveChangesAsync(ct);
    }
}
