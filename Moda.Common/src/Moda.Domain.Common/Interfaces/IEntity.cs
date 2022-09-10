namespace Moda.Common.Domain.Interfaces;

public interface IEntity<TId>
{
    TId Id { get; }
}
