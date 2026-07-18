using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Expenses.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Expenses.Commands;

public record CreateExpenseCommand : IRequest<ExpenseDto>
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

public class CreateExpenseCommandHandler : IRequestHandler<CreateExpenseCommand, ExpenseDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateExpenseCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ExpenseDto> Handle(CreateExpenseCommand request, CancellationToken ct)
    {
        var category = await _context.ExpenseCategories.FirstOrDefaultAsync(c => c.Id == request.CategoryId, ct);
        if (category is null) throw new NotFoundException(nameof(ExpenseCategory), request.CategoryId);

        var entity = new Expense
        {
            Reference = request.Reference,
            Amount = request.Amount,
            ExpenseDate = request.ExpenseDate == default ? DateTime.UtcNow : request.ExpenseDate,
            Description = request.Description,
            Vendor = request.Vendor,
            Status = "PENDING",
            PaymentMethod = request.PaymentMethod,
            CategoryId = request.CategoryId,
            UserId = _currentUser.UserId,
            PurchaseOrderId = request.PurchaseOrderId,
            SaleId = request.SaleId,
        };

        _context.Expenses.Add(entity);
        await _context.SaveChangesAsync(ct);

        var created = await _context.Expenses
            .Include(e => e.Category)
            .Include(e => e.User)
            .FirstAsync(e => e.Id == entity.Id, ct);

        return MapToDto(created);
    }

    private static ExpenseDto MapToDto(Expense e) => new()
    {
        Id = e.Id,
        Reference = e.Reference,
        Amount = e.Amount,
        ExpenseDate = e.ExpenseDate,
        Description = e.Description,
        Vendor = e.Vendor,
        Status = e.Status,
        PaymentMethod = e.PaymentMethod,
        CategoryId = e.CategoryId,
        Category = e.Category is null ? null : new ExpenseCategoryDto
        {
            Id = e.Category.Id,
            Name = e.Category.Name,
            Description = e.Category.Description,
            CompanyId = e.Category.CompanyId,
        },
        UserId = e.UserId,
        User = e.User is null ? null : new UserBriefDto
        {
            Id = e.User.Id,
            Name = e.User.Name,
            Email = e.User.Email,
        },
        PurchaseOrderId = e.PurchaseOrderId,
        SaleId = e.SaleId,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt,
        Attachments = e.Attachments?.Select(a => new ExpenseAttachmentDto
        {
            Id = a.Id,
            FileName = a.FileName,
            FileType = a.FileType,
            Url = a.Url,
            CreatedAt = a.CreatedAt,
        }).ToList() ?? [],
    };
}

public class CreateExpenseCommandValidator : AbstractValidator<CreateExpenseCommand>
{
    public CreateExpenseCommandValidator()
    {
        RuleFor(v => v.Reference).NotEmpty().MaximumLength(100);
        RuleFor(v => v.Amount).GreaterThan(0);
        RuleFor(v => v.Vendor).NotEmpty().MaximumLength(200);
        RuleFor(v => v.CategoryId).NotEmpty();
    }
}
