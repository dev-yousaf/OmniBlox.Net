using Microsoft.EntityFrameworkCore;
using OmniBlox.Application.Common.Interfaces;
using OmniBlox.Domain.Entities;
using OmniBlox.Shared.Exceptions;

namespace OmniBlox.Infrastructure.Services;

public class CrudService<TEntity, TDto> : ICrudService<TEntity, TDto>
    where TEntity : BaseEntity
{
    private readonly IApplicationDbContext _context;

    public CrudService(IApplicationDbContext context) => _context = context;

    public async Task<TDto> CreateAsync(TEntity entity, Func<TEntity, TDto> toDto, CancellationToken ct)
    {
        _context.Set<TEntity>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return toDto(entity);
    }

    public async Task<TDto> UpdateAsync(Guid id, Action<TEntity> applyUpdates, Func<TEntity, TDto> toDto, CancellationToken ct)
    {
        var entity = await _context.Set<TEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) throw new NotFoundException(typeof(TEntity).Name, id);
        applyUpdates(entity);
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return toDto(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var entity = await _context.Set<TEntity>().AsTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) throw new NotFoundException(typeof(TEntity).Name, id);
        _context.Set<TEntity>().Remove(entity);
        await _context.SaveChangesAsync(ct);
    }
}
