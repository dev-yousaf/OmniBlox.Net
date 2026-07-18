namespace OmniBlox.Application.Features.Expenses.DTOs;

public record ExpenseDto
{
    public Guid Id { get; init; }
    public string Reference { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime ExpenseDate { get; init; }
    public string? Description { get; init; }
    public string Vendor { get; init; } = string.Empty;
    public string Status { get; init; } = "PENDING";
    public string? PaymentMethod { get; init; }
    public Guid CategoryId { get; init; }
    public ExpenseCategoryDto? Category { get; init; }
    public Guid UserId { get; init; }
    public UserBriefDto? User { get; init; }
    public Guid? PurchaseOrderId { get; init; }
    public Guid? SaleId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public List<ExpenseAttachmentDto> Attachments { get; init; } = [];
}

public record ExpenseAttachmentDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string FileType { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public record UserBriefDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}

public record CreateExpenseDto
{
    public string Reference { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime ExpenseDate { get; init; }
    public string? Description { get; init; }
    public string Vendor { get; init; } = string.Empty;
    public string? PaymentMethod { get; init; }
    public Guid CategoryId { get; init; }
    public Guid? PurchaseOrderId { get; init; }
    public Guid? SaleId { get; init; }
}

public record UpdateExpenseDto
{
    public string? Reference { get; init; }
    public decimal? Amount { get; init; }
    public DateTime? ExpenseDate { get; init; }
    public string? Description { get; init; }
    public string? Vendor { get; init; }
    public string? PaymentMethod { get; init; }
    public Guid? CategoryId { get; init; }
}

public record UpdateExpenseStatusDto
{
    public string Status { get; init; } = "PENDING";
}

public record ExpenseStatsDto
{
    public int TotalExpenses { get; init; }
    public int PendingExpenses { get; init; }
    public int ApprovedExpenses { get; init; }
    public int PaidExpenses { get; init; }
    public int RejectedExpenses { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal PendingAmount { get; init; }
    public decimal ApprovedAmount { get; init; }
    public decimal PaidAmount { get; init; }
}
