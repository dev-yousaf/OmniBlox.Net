using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Expenses.DTOs;

namespace OmniBlox.Application.Features.ExpenseCategories.Queries;

public record GetExpenseCategoriesQuery : IRequest<List<ExpenseCategoryDto>>;

public class GetExpenseCategoriesQueryHandler : IRequestHandler<GetExpenseCategoriesQuery, List<ExpenseCategoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetExpenseCategoriesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<ExpenseCategoryDto>> Handle(GetExpenseCategoriesQuery request, CancellationToken ct)
    {
        var items = await _context.ExpenseCategories
            .Where(e => e.CompanyId == _currentUser.CompanyId)
            .OrderBy(x => x.Name)
            .ToListAsync(ct);

        return items.Select(e => new ExpenseCategoryDto
        {
            Id = e.Id,
            Name = e.Name,
            Description = e.Description,
            CompanyId = e.CompanyId,
        }).ToList();
    }
}
