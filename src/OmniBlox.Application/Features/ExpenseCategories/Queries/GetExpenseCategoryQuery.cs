using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Expenses.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.ExpenseCategories.Queries;

public record GetExpenseCategoryQuery : IRequest<ExpenseCategoryDto>
{
    public Guid Id { get; init; }
}

public class GetExpenseCategoryQueryHandler : IRequestHandler<GetExpenseCategoryQuery, ExpenseCategoryDto>
{
    private readonly IApplicationDbContext _context;
    public GetExpenseCategoryQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<ExpenseCategoryDto> Handle(GetExpenseCategoryQuery request, CancellationToken ct)
    {
        var entity = await _context.ExpenseCategories.FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (entity is null) throw new NotFoundException(nameof(ExpenseCategory), request.Id);

        return new ExpenseCategoryDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            CompanyId = entity.CompanyId,
        };
    }
}
