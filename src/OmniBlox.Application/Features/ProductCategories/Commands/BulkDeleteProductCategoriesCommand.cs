using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.ProductCategories.DTOs;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Application.Features.ProductCategories.Commands;

public record BulkDeleteProductCategoriesCommand : IRequest<BulkDeleteResponse>
{
    public List<Guid> Ids { get; init; } = [];
}

public class BulkDeleteProductCategoriesCommandHandler : IRequestHandler<BulkDeleteProductCategoriesCommand, BulkDeleteResponse>
{
    private readonly IApplicationDbContext _context;
    public BulkDeleteProductCategoriesCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<BulkDeleteResponse> Handle(BulkDeleteProductCategoriesCommand request, CancellationToken ct)
    {
        var deleted = new List<Guid>();
        var failed = new List<FailedItem>();

        foreach (var id in request.Ids)
        {
            var entity = await _context.ProductCategories.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                failed.Add(new FailedItem { Id = id, Error = "Not found" });
                continue;
            }
            _context.ProductCategories.Remove(entity);
            deleted.Add(id);
        }

        await _context.SaveChangesAsync(ct);
        return new BulkDeleteResponse
        {
            Message = $"{deleted.Count} category(s) deleted, {failed.Count} failed.",
            Deleted = deleted,
            Failed = failed,
        };
    }
}
