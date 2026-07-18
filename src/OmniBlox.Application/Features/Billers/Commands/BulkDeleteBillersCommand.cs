using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;

namespace OmniBlox.Application.Features.Billers.Commands;

public record BulkDeleteBillersCommand : IRequest<BulkDeleteResponse>
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

public class BulkDeleteBillersCommandHandler : IRequestHandler<BulkDeleteBillersCommand, BulkDeleteResponse>
{
    private readonly IApplicationDbContext _context;
    public BulkDeleteBillersCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<BulkDeleteResponse> Handle(BulkDeleteBillersCommand request, CancellationToken ct)
    {
        var deleted = new List<Guid>();
        var failed = new List<FailedItem>();

        foreach (var id in request.Ids)
        {
            var entity = await _context.Billers.AsTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null) { failed.Add(new FailedItem { Id = id, Error = "Not found" }); continue; }
            _context.Billers.Remove(entity);
            deleted.Add(id);
        }

        await _context.SaveChangesAsync(ct);
        return new BulkDeleteResponse { Message = $"{deleted.Count} biller(s) deleted, {failed.Count} failed.", Deleted = deleted, Failed = failed };
    }
}
