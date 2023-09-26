using Moda.Common.Application.Interfaces;
using NodaTime;

namespace Moda.Tests.Shared;
public class TestingDateTimeService : IDateTimeService
{
    private readonly IClock _clock;

    public TestingDateTimeService(IClock clock)
    {
        _clock = clock;
    }

    public Instant Now => _clock.GetCurrentInstant();

    public LocalDate Today => _clock.GetCurrentInstant().InUtc().Date;
}
