namespace Wayd.Common.Application.Interfaces;

public interface IDateTimeProvider : IScopedService
{
    Instant Now { get; }

    LocalDate Today { get; }
}
