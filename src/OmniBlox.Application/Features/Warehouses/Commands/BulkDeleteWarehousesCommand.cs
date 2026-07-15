using MediatR;
using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Application.Features.Brands.Commands;
using OmniBlox.Domain.Entities;

namespace OmniBlox.Application.Features.Warehouses.Commands;

public record BulkDeleteWarehousesCommand : IRequest<BulkDeleteResponse>
{
    public List<Guid> Ids { get; init; } = [];
}

public class BulkDeleteWarehousesCommandHandler : IRequestHandler<BulkDeleteWarehousesCommand, BulkDeleteResponse>
{
    private readonly IApplicationDbContext _context;
    public BulkDeleteWarehousesCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<BulkDeleteResponse> Handle(BulkDeleteWarehousesCommand request, CancellationToken ct)
    {
        var deleted = new List<Guid>();
        var failed = new List<FailedItem>();

        foreach (var id in request.Ids)
        {
            var entity = await _context.Warehouses.Include(w => w.Inventories).FirstOrDefaultAsync(x => x.Id == id, ct);
            if (entity is null) { failed.Add(new FailedItem { Id = id, Error = "Not found" }); continue; }
            if (entity.Inventories.Count > 0) { failed.Add(new FailedItem { Id = id, Error = "Has existing inventory" }); continue; }
            _context.Warehouses.Remove(entity);
            deleted.Add(id);
        }

        await _context.SaveChangesAsync(ct);
        return new BulkDeleteResponse { Message = $"{deleted.Count} warehouse(s) deleted, {failed.Count} failed.", Deleted = deleted, Failed = failed };
    }
}
