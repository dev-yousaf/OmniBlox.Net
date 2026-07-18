using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Expenses.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.ExpenseCategories.Commands;

public record UpdateExpenseCategoryCommand : IRequest<ExpenseCategoryDto>
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
}

public class UpdateExpenseCategoryCommandHandler : IRequestHandler<UpdateExpenseCategoryCommand, ExpenseCategoryDto>
{
    private readonly IApplicationDbContext _context;
    public UpdateExpenseCategoryCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<ExpenseCategoryDto> Handle(UpdateExpenseCategoryCommand request, CancellationToken ct)
    {
        var entity = await _context.ExpenseCategories.AsTracking().FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null) throw new NotFoundException(nameof(ExpenseCategory), request.Id);

        if (request.Name is not null) entity.Name = request.Name;
        if (request.Description is not null) entity.Description = request.Description;

        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return new ExpenseCategoryDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            CompanyId = entity.CompanyId,
        };
    }
}

public class UpdateExpenseCategoryCommandValidator : AbstractValidator<UpdateExpenseCategoryCommand>
{
    public UpdateExpenseCategoryCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
    }
}
