namespace Moda.Common.Domain.Interfaces;

public interface IHasIdAndKey<TKey>
{
    Guid Id { get; }
    TKey Key { get; }
} 


public interface IHasIdAndKey
{
    Guid Id { get; }
    int Key { get; }
}
