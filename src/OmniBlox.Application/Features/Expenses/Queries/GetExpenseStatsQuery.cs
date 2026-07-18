using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Expenses.DTOs;

namespace OmniBlox.Application.Features.Expenses.Queries;

public record GetExpenseStatsQuery : IRequest<ExpenseStatsDto>;

public class GetExpenseStatsQueryHandler : IRequestHandler<GetExpenseStatsQuery, ExpenseStatsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetExpenseStatsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ExpenseStatsDto> Handle(GetExpenseStatsQuery request, CancellationToken ct)
    {
        var expenses = await _context.Expenses
            .Where(e => e.CompanyId == _currentUser.CompanyId)
            .ToListAsync(ct);

        return new ExpenseStatsDto
        {
            TotalExpenses = expenses.Count,
            PendingExpenses = expenses.Count(e => e.Status == "PENDING"),
            ApprovedExpenses = expenses.Count(e => e.Status == "APPROVED"),
            PaidExpenses = expenses.Count(e => e.Status == "PAID"),
            RejectedExpenses = expenses.Count(e => e.Status == "REJECTED"),
            TotalAmount = expenses.Sum(e => e.Amount),
            PendingAmount = expenses.Where(e => e.Status == "PENDING").Sum(e => e.Amount),
            ApprovedAmount = expenses.Where(e => e.Status == "APPROVED").Sum(e => e.Amount),
            PaidAmount = expenses.Where(e => e.Status == "PAID").Sum(e => e.Amount),
        };
    }
}
