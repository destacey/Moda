using NodaTime;

namespace Moda.Application.Common.Interfaces;

public interface IDateTimeService
{
    Instant Now { get; }
}
