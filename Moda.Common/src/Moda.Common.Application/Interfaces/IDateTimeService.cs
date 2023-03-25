using NodaTime;

namespace Moda.Common.Application.Interfaces;

public interface IDateTimeService : IScopedService
{
    Instant Now { get; }
}
