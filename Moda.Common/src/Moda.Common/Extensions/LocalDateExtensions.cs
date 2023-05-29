using NodaTime;

namespace Moda.Common.Extensions;
public static class LocalDateExtensions
{
    public static Instant ToInstant(this LocalDate localDate)
    {
        return new LocalDateTime(localDate.Year, localDate.Month, localDate.Day, 0, 0, 0)
            .InUtc().ToInstant();
    }
}
