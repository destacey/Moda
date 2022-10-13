using Ardalis.Specification;

namespace Moda.Infrastructure.Persistence.Repository;

/// <summary>
/// The repository that implements IRepositoryWithEvents.
/// Implemented as a decorator. It only augments the Add,
/// Update and Delete calls where it adds the respective
/// EntityCreated, EntityUpdated or EntityDeleted event
/// before delegating to the decorated repository.
/// </summary>
public class EventAddingRepositoryDecorator<T> : IRepositoryWithEvents<T>
    where T : class, IAggregateRoot
{
    private readonly IRepository<T> _decorated;
    private readonly IDateTimeService _dateTimeService;

    public EventAddingRepositoryDecorator(IRepository<T> decorated, IDateTimeService dateTimeService)
    {
        _decorated = decorated;
        _dateTimeService = dateTimeService;
    }

    public Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.AddDomainEvent(EntityCreatedEvent.WithEntity(entity, _dateTimeService.Now));
        return _decorated.AddAsync(entity, cancellationToken);
    }

    public Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            entity.AddDomainEvent(EntityCreatedEvent.WithEntity(entity, _dateTimeService.Now));
        }
        return _decorated.AddRangeAsync(entities, cancellationToken);
    }

    public Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.AddDomainEvent(EntityUpdatedEvent.WithEntity(entity, _dateTimeService.Now));
        return _decorated.UpdateAsync(entity, cancellationToken);
    }

    public Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            entity.AddDomainEvent(EntityUpdatedEvent.WithEntity(entity, _dateTimeService.Now));
        }
        return _decorated.UpdateRangeAsync(entities, cancellationToken);
    }

    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.AddDomainEvent(EntityDeletedEvent.WithEntity(entity, _dateTimeService.Now));
        return _decorated.DeleteAsync(entity, cancellationToken);
    }

    public Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            entity.AddDomainEvent(EntityDeletedEvent.WithEntity(entity, _dateTimeService.Now));
        }

        return _decorated.DeleteRangeAsync(entities, cancellationToken);
    }

    // The rest of the methods are simply forwarded.
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _decorated.SaveChangesAsync(cancellationToken);

    public Task<T?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = default)
        where TId : notnull =>
        _decorated.GetByIdAsync(id, cancellationToken);

    [Obsolete]
    public Task<T?> GetBySpecAsync(ISpecification<T> specification, CancellationToken cancellationToken = default) =>
        _decorated.GetBySpecAsync(specification, cancellationToken);

    [Obsolete]
    public Task<TResult?> GetBySpecAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default) =>
        _decorated.GetBySpecAsync(specification, cancellationToken);

    public Task<T?> FirstOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = default) =>
        _decorated.FirstOrDefaultAsync(specification, cancellationToken);

    public Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default) =>
        _decorated.FirstOrDefaultAsync(specification, cancellationToken);
    public Task<T?> SingleOrDefaultAsync(ISingleResultSpecification<T> specification, CancellationToken cancellationToken = default) =>
        _decorated.SingleOrDefaultAsync(specification, cancellationToken);

    public Task<TResult?> SingleOrDefaultAsync<TResult>(ISingleResultSpecification<T, TResult> specification, CancellationToken cancellationToken = default) =>
        _decorated.SingleOrDefaultAsync(specification, cancellationToken);

    public Task<List<T>> ListAsync(CancellationToken cancellationToken = default) =>
        _decorated.ListAsync(cancellationToken);

    public Task<List<T>> ListAsync(ISpecification<T> specification, CancellationToken cancellationToken = default) =>
        _decorated.ListAsync(specification, cancellationToken);

    public Task<List<TResult>> ListAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default) =>
        _decorated.ListAsync(specification, cancellationToken);

    public Task<bool> AnyAsync(ISpecification<T> specification, CancellationToken cancellationToken = default) =>
        _decorated.AnyAsync(specification, cancellationToken);

    public Task<bool> AnyAsync(CancellationToken cancellationToken = default) =>
        _decorated.AnyAsync(cancellationToken);

    public Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default) =>
        _decorated.CountAsync(specification, cancellationToken);

    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        _decorated.CountAsync(cancellationToken);
}