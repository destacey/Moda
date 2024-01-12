using NodaTime;

namespace Moda.Infrastructure.Common.Services;
public class DateTimeProvider : IDateTimeProvider
{
    private readonly IClock _clock;

    public DateTimeProvider(IClock clock)
    {
        _clock = clock;
    }

    public Instant Now => _clock.GetCurrentInstant();
}
