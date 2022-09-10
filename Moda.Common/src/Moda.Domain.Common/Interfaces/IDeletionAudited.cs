using NodaTime;

namespace Moda.Common.Domain.Interfaces;

public interface IDeletionAudited : ISoftDelete
{
    Instant Deleted { get; }

    string? DeletedBy { get; }
}
