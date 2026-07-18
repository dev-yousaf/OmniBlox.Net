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
    private readonly ICurrentUserService _currentUser;
    private readonly ICrudService<ExpenseCategory, ExpenseCategoryDto> _crud;
    public CreateExpenseCategoryCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, ICrudService<ExpenseCategory, ExpenseCategoryDto> crud)
    {
        _context = context;
        _currentUser = currentUser;
        _crud = crud;
    }

    public async Task<ExpenseCategoryDto> Handle(CreateExpenseCategoryCommand request, CancellationToken ct)
    {
        var companyId = _currentUser.CompanyId;

        var entity = new ExpenseCategory
        {
            Name = request.Name,
            Description = request.Description,
            CompanyId = companyId,
        };

        return await _crud.CreateAsync(entity, e => new ExpenseCategoryDto
        {
            Id = e.Id,
            Name = e.Name,
            Description = e.Description,
            CompanyId = e.CompanyId,
        }, ct);
    }
}

public class CreateExpenseCategoryCommandValidator : AbstractValidator<CreateExpenseCategoryCommand>
{
    public CreateExpenseCategoryCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(100);
    }
}
