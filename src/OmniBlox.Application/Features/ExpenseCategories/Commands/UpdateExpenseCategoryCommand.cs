using FluentValidation;
using MediatR;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Expenses.DTOs;
using OmniBlox.Domain.Entities;

namespace OmniBlox.Application.Features.ExpenseCategories.Commands;

public record UpdateExpenseCategoryCommand : IRequest<ExpenseCategoryDto>
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
}

public class UpdateExpenseCategoryCommandHandler : IRequestHandler<UpdateExpenseCategoryCommand, ExpenseCategoryDto>
{
    private readonly ICrudService<ExpenseCategory, ExpenseCategoryDto> _crud;
    public UpdateExpenseCategoryCommandHandler(ICrudService<ExpenseCategory, ExpenseCategoryDto> crud) => _crud = crud;

    public async Task<ExpenseCategoryDto> Handle(UpdateExpenseCategoryCommand request, CancellationToken ct)
    {
        return await _crud.UpdateAsync(request.Id, entity =>
        {
            if (request.Name is not null) entity.Name = request.Name;
            if (request.Description is not null) entity.Description = request.Description;
        }, e => new ExpenseCategoryDto
        {
            Id = e.Id,
            Name = e.Name,
            Description = e.Description,
            CompanyId = e.CompanyId,
        }, ct);
    }
}

public class UpdateExpenseCategoryCommandValidator : AbstractValidator<UpdateExpenseCategoryCommand>
{
    public UpdateExpenseCategoryCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
    }
}
