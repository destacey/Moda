namespace Moda.Common.Domain.Interfaces;
public interface IHasIdAndKey
{
    Guid Id { get; }
    int Key { get; }
}
