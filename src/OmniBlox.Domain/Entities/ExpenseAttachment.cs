namespace OmniBlox.Domain.Entities;

public class ExpenseAttachment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public Guid ExpenseId { get; set; }
    public Expense Expense { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
