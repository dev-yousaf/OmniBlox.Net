using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Expenses.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.Expenses.Queries;

public record GetExpenseQuery : IRequest<ExpenseDto>
{
    public Guid Id { get; init; }
}

public class GetExpenseQueryHandler : IRequestHandler<GetExpenseQuery, ExpenseDto>
{
    private readonly IApplicationDbContext _context;

    public GetExpenseQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExpenseDto> Handle(GetExpenseQuery request, CancellationToken ct)
    {
        var entity = await _context.Expenses
            .Include(e => e.Category)
            .Include(e => e.User)
            .Include(e => e.Attachments)
            .FirstOrDefaultAsync(e => e.Id == request.Id, ct);
        if (entity is null) throw new NotFoundException(nameof(Expense), request.Id);

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
            Attachments = entity.Attachments.Select(a => new ExpenseAttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                FileType = a.FileType,
                Url = a.Url,
                CreatedAt = a.CreatedAt,
            }).ToList(),
        };
    }
}
