using NodaTime;

namespace Moda.Common.Domain.Interfaces;

public interface ISoftDelete
{
    bool IsDeleted { get; set; }

    Instant? Deleted { get; set; }

    Guid? DeletedBy { get; set; }
}
