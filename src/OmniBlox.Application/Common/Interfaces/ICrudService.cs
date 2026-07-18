using OmniBlox.Domain.Entities;

namespace OmniBlox.Application.Common.Interfaces;

public interface ICrudService<TEntity, TDto>
    where TEntity : BaseEntity
{
    Task<TDto> CreateAsync(TEntity entity, Func<TEntity, TDto> toDto, CancellationToken ct);
    Task<TDto> UpdateAsync(Guid id, Action<TEntity> applyUpdates, Func<TEntity, TDto> toDto, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
}
