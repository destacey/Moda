using NodaTime;

namespace Moda.Infrastructure.Common.Services;
public class DateTimeService : IDateTimeService
{
    private readonly IClock _clock;

    public DateTimeService(IClock clock)
    {
        _clock = clock;
    }

    public Instant Now => _clock.GetCurrentInstant();
}
