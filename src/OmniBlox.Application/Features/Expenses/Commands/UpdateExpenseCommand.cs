using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Expenses.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Expenses.Commands;

public record UpdateExpenseCommand : IRequest<ExpenseDto>
{
    public Guid Id { get; init; }
    public string? Reference { get; init; }
    public decimal? Amount { get; init; }
    public DateTime? ExpenseDate { get; init; }
    public string? Description { get; init; }
    public string? Vendor { get; init; }
    public string? PaymentMethod { get; init; }
    public Guid? CategoryId { get; init; }
}

public class UpdateExpenseCommandHandler : IRequestHandler<UpdateExpenseCommand, ExpenseDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateExpenseCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExpenseDto> Handle(UpdateExpenseCommand request, CancellationToken ct)
    {
        var entity = await _context.Expenses
            .Include(e => e.Category)
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == request.Id, ct);
        if (entity is null) throw new NotFoundException(nameof(Expense), request.Id);

        if (request.Reference is not null) entity.Reference = request.Reference;
        if (request.Amount.HasValue) entity.Amount = request.Amount.Value;
        if (request.ExpenseDate.HasValue) entity.ExpenseDate = request.ExpenseDate.Value;
        if (request.Description is not null) entity.Description = request.Description;
        if (request.Vendor is not null) entity.Vendor = request.Vendor;
        if (request.PaymentMethod is not null) entity.PaymentMethod = request.PaymentMethod;
        if (request.CategoryId.HasValue)
        {
            var category = await _context.ExpenseCategories.FirstOrDefaultAsync(c => c.Id == request.CategoryId.Value, ct);
            if (category is null) throw new NotFoundException(nameof(ExpenseCategory), request.CategoryId.Value);
            entity.CategoryId = request.CategoryId.Value;
        }

        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return MapToDto(entity);
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

public class UpdateExpenseCommandValidator : AbstractValidator<UpdateExpenseCommand>
{
    public UpdateExpenseCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.Reference).MaximumLength(100).When(v => v.Reference is not null);
        RuleFor(v => v.Amount).GreaterThan(0).When(v => v.Amount.HasValue);
        RuleFor(v => v.Vendor).MaximumLength(200).When(v => v.Vendor is not null);
    }
}
