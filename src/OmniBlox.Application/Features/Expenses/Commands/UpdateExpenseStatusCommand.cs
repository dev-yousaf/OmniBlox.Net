using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Expenses.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Expenses.Commands;

public record UpdateExpenseStatusCommand : IRequest<ExpenseDto>
{
    public Guid Id { get; init; }
    public string Status { get; init; } = "PENDING";
}

public class UpdateExpenseStatusCommandHandler : IRequestHandler<UpdateExpenseStatusCommand, ExpenseDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateExpenseStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExpenseDto> Handle(UpdateExpenseStatusCommand request, CancellationToken ct)
    {
        var entity = await _context.Expenses
            .Include(e => e.Category)
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == request.Id, ct);
        if (entity is null) throw new NotFoundException(nameof(Expense), request.Id);

        entity.Status = request.Status;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return new ExpenseDto
        {
            Id = entity.Id,
            Reference = entity.Reference,
            Amount = entity.Amount,
            ExpenseDate = entity.ExpenseDate,
            Description = entity.Description,
            Vendor = entity.Vendor,
            Status = entity.Status,
            PaymentMethod = entity.PaymentMethod,
            CategoryId = entity.CategoryId,
            Category = entity.Category is null ? null : new ExpenseCategoryDto
            {
                Id = entity.Category.Id,
                Name = entity.Category.Name,
                Description = entity.Category.Description,
                CompanyId = entity.Category.CompanyId,
            },
            UserId = entity.UserId,
            User = entity.User is null ? null : new UserBriefDto
            {
                Id = entity.User.Id,
                Name = entity.User.Name,
                Email = entity.User.Email,
            },
            PurchaseOrderId = entity.PurchaseOrderId,
            SaleId = entity.SaleId,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
        };
    }
}

public class UpdateExpenseStatusCommandValidator : AbstractValidator<UpdateExpenseStatusCommand>
{
    public UpdateExpenseStatusCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.Status).NotEmpty().Must(s =>
            s == "PENDING" || s == "APPROVED" || s == "PAID" || s == "REJECTED");
    }
}
