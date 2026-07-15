using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;

namespace OmniBlox.Application.Features.Units.Commands;

public record BulkDeleteUnitsCommand : IRequest<BulkDeleteResponse>
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

public class BulkDeleteUnitsCommandHandler : IRequestHandler<BulkDeleteUnitsCommand, BulkDeleteResponse>
{
    private readonly IApplicationDbContext _context;
    public BulkDeleteUnitsCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<BulkDeleteResponse> Handle(BulkDeleteUnitsCommand request, CancellationToken ct)
    {
        var deleted = new List<Guid>();
        var failed = new List<FailedItem>();
        foreach (var id in request.Ids)
        {
            var entity = await _context.Units.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null) { failed.Add(new FailedItem { Id = id, Error = "Not found" }); continue; }
            _context.Units.Remove(entity);
            deleted.Add(id);
            deleted.Add(id);
        }
        await _context.SaveChangesAsync(ct);
        return new BulkDeleteResponse { Message = $"{deleted.Count} unit(s) deleted, {failed.Count} failed.", Deleted = deleted, Failed = failed };
    }
}
