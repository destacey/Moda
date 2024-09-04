namespace Moda.Common.Domain.Interfaces;
public interface HasIdAndKey
{
    Guid Id { get; }
    int Key { get; }
}
