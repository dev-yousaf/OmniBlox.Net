using OmniBlox.Application.Features.Expenses.DTOs;

namespace OmniBlox.Application.Features.ExpenseCategories.DTOs;

// Reuse CreateExpenseCategoryDto and UpdateExpenseCategoryDto from Expenses feature
public record ExpenseCategoryDetailDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid CompanyId { get; init; }
    public List<ExpenseDto> Expenses { get; init; } = [];
}
