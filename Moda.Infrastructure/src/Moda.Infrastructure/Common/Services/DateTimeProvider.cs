using NodaTime;

namespace Moda.Infrastructure.Common.Services;
public class DateTimeProvider(IClock clock) : IDateTimeProvider
{
    private readonly IClock _clock = clock;

    public Instant Now => _clock.GetCurrentInstant();

    public LocalDate Today => _clock.GetCurrentInstant().InUtc().Date;
}
