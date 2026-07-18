using MediatR;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Expenses.DTOs;
using OmniBlox.Domain.Entities;

namespace OmniBlox.Application.Features.ExpenseCategories.Commands;

public record DeleteExpenseCategoryCommand : IRequest
{
    public Guid Id { get; init; }
}

public class DeleteExpenseCategoryCommandHandler : IRequestHandler<DeleteExpenseCategoryCommand>
{
    private readonly ICrudService<ExpenseCategory, ExpenseCategoryDto> _crud;
    public DeleteExpenseCategoryCommandHandler(ICrudService<ExpenseCategory, ExpenseCategoryDto> crud) => _crud = crud;

    public async Task Handle(DeleteExpenseCategoryCommand request, CancellationToken ct)
    {
        await _crud.DeleteAsync(request.Id, ct);
    }
}
