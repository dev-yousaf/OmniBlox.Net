using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Expenses.DTOs;

namespace OmniBlox.Application.Features.Expenses.Queries;

public record GetExpensesQuery : IRequest<List<ExpenseDto>>
{
    public string? Search { get; init; }
    public string? Status { get; init; }
    public Guid? CategoryId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

public class GetExpensesQueryHandler : IRequestHandler<GetExpensesQuery, List<ExpenseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetExpensesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<ExpenseDto>> Handle(GetExpensesQuery request, CancellationToken ct)
    {
        var query = _context.Expenses
            .Include(e => e.Category)
            .Include(e => e.User)
            .Where(e => e.CompanyId == _currentUser.CompanyId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.ToLower();
            query = query.Where(e =>
                e.Reference.ToLower().Contains(s) ||
                e.Vendor.ToLower().Contains(s) ||
                e.Description!.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(e => e.Status == request.Status);

        if (request.CategoryId.HasValue)
            query = query.Where(e => e.CategoryId == request.CategoryId.Value);

        if (request.FromDate.HasValue)
            query = query.Where(e => e.ExpenseDate >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(e => e.ExpenseDate <= request.ToDate.Value);

        return await query
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new ExpenseDto
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
                Category = e.Category == null ? null : new ExpenseCategoryDto
                {
                    Id = e.Category.Id,
                    Name = e.Category.Name,
                    Description = e.Category.Description,
                    CompanyId = e.Category.CompanyId,
                },
                UserId = e.UserId,
                User = e.User == null ? null : new UserBriefDto
                {
                    Id = e.User.Id,
                    Name = e.User.Name,
                    Email = e.User.Email,
                },
                PurchaseOrderId = e.PurchaseOrderId,
                SaleId = e.SaleId,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
            })
            .ToListAsync(ct);
    }
}
