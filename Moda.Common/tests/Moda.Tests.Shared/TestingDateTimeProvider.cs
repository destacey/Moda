using Moda.Common.Application.Interfaces;
using NodaTime;
using NodaTime.Testing;

namespace Moda.Tests.Shared;
public class TestingDateTimeProvider : IDateTimeProvider
{
    private readonly FakeClock _clock;

    public TestingDateTimeProvider(IClock clock)
    {
        _clock = (FakeClock)clock;
    }

    public Instant Now => _clock.GetCurrentInstant();

    public LocalDate Today => _clock.GetCurrentInstant().InUtc().Date;

    public void Advance(Duration duration)
    {
        _clock.Advance(duration);
    }
}
