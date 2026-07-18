using FluentValidation;
using MediatR;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Expenses.DTOs;
using OmniBlox.Domain.Entities;

namespace OmniBlox.Application.Features.ExpenseCategories.Commands;

public record CreateExpenseCategoryCommand : IRequest<ExpenseCategoryDto>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public class CreateExpenseCategoryCommandHandler : IRequestHandler<CreateExpenseCategoryCommand, ExpenseCategoryDto>
{
    private readonly IApplicationDbContext _context;
    public CreateExpenseCategoryCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<ExpenseCategoryDto> Handle(CreateExpenseCategoryCommand request, CancellationToken ct)
    {
        var entity = new ExpenseCategory
        {
            Name = request.Name,
            Description = request.Description,
        };

        _context.ExpenseCategories.Add(entity);
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

public class CreateExpenseCategoryCommandValidator : AbstractValidator<CreateExpenseCategoryCommand>
{
    public CreateExpenseCategoryCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(100);
    }
}
