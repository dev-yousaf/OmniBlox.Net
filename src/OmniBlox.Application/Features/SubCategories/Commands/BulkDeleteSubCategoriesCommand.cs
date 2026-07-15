using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;

namespace OmniBlox.Application.Features.SubCategories.Commands;

public record BulkDeleteSubCategoriesCommand : IRequest<BulkDeleteResponse>
{
    public List<Guid> Ids { get; init; } = [];
}

public record BulkDeleteResponse
{
    public string Message { get; init; } = string.Empty;
    public List<Guid> Deleted { get; init; } = [];
    public List<FailedItem> Failed { get; init; } = [];
}

public record FailedItem
{
    public Guid Id { get; init; }
    public string Error { get; init; } = string.Empty;
}

public class BulkDeleteSubCategoriesCommandHandler : IRequestHandler<BulkDeleteSubCategoriesCommand, BulkDeleteResponse>
{
    private readonly IApplicationDbContext _context;
    public BulkDeleteSubCategoriesCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<BulkDeleteResponse> Handle(BulkDeleteSubCategoriesCommand request, CancellationToken ct)
    {
        var deleted = new List<Guid>();
        var failed = new List<FailedItem>();

        foreach (var id in request.Ids)
        {
            var entity = await _context.SubCategories.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null)
            {
                failed.Add(new FailedItem { Id = id, Error = "Not found" });
                continue;
            }
            _context.SubCategories.Remove(entity);
            deleted.Add(id);
        }

        await _context.SaveChangesAsync(ct);
        return new BulkDeleteResponse
        {
            Message = $"{deleted.Count} sub-category(s) deleted, {failed.Count} failed.",
            Deleted = deleted,
            Failed = failed,
        };
    }
}
