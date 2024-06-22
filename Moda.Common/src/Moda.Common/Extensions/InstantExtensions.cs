using NodaTime;

namespace Moda.Common.Extensions;
public static class InstantExtensions
{
    /// <summary>
    /// Converts the Instant to a DateOnly based on the UTC time.
    /// </summary>
    /// <param name="instant"></param>
    /// <returns></returns>
    public static DateOnly ToDateOnly(this Instant instant)
    {
        return DateOnly.FromDateTime(instant.ToDateTimeUtc());
    }
}
