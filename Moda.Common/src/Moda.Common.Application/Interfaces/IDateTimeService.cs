using NodaTime;

namespace Moda.Common.Application.Interfaces;

public interface IDateTimeService : ITransientService
{
    Instant Now { get; }
}
