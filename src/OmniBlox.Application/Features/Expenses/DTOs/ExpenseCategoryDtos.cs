namespace OmniBlox.Application.Features.Expenses.DTOs;

public record ExpenseCategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid CompanyId { get; init; }
}

public record CreateExpenseCategoryDto
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public record UpdateExpenseCategoryDto
{
    public string? Name { get; init; }
    public string? Description { get; init; }
}
