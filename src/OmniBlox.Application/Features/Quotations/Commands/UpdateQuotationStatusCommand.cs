using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Quotations.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Quotations.Commands;

public record UpdateQuotationStatusCommand : IRequest<QuotationDetailDto>
{
    public Guid Id { get; init; }
    public string Status { get; init; } = string.Empty;
}

public class UpdateQuotationStatusCommandHandler : IRequestHandler<UpdateQuotationStatusCommand, QuotationDetailDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateQuotationStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<QuotationDetailDto> Handle(UpdateQuotationStatusCommand request, CancellationToken ct)
    {
        var quotation = await _context.Quotations
            .Include(q => q.Customer)
            .Include(q => q.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(q => q.Id == request.Id, ct);

        if (quotation is null)
            throw new NotFoundException(nameof(Quotation), request.Id);

        quotation.Status = request.Status;
        quotation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return QuotationDetailDto.FromEntity(quotation);
    }
}

public class UpdateQuotationStatusCommandValidator : AbstractValidator<UpdateQuotationStatusCommand>
{
    public UpdateQuotationStatusCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.Status).NotEmpty().Must(s =>
            s is "DRAFT" or "PENDING" or "COMPLETED" or "CANCELLED");
    }
}
