using NodaTime;

namespace Moda.Common.Application.Interfaces;

public interface IDateTimeService
{
    Instant Now { get; }
}
