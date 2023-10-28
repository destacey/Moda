using Moda.Common.Application.Interfaces;
using NodaTime;
using NodaTime.Testing;

namespace Moda.Tests.Shared;
public class TestingDateTimeService : IDateTimeService
{
    private readonly FakeClock _clock;

    public TestingDateTimeService(IClock clock)
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
