using NodaTime;

namespace Moda.Common.Domain.Interfaces;

public interface IHasModificationDateTime
{
    Instant LastModified { get; }
}
