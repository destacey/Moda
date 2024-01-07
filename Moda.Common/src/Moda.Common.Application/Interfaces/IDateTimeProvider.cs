namespace Moda.Common.Application.Interfaces;

public interface IDateTimeProvider : IScopedService
{
    Instant Now { get; }
}
