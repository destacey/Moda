using NodaTime;

namespace Moda.Common.Domain.Interfaces;

public interface IHasCreationDateTime
{
    Instant Created { get; }
}
