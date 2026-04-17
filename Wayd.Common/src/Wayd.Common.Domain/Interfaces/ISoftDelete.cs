using NodaTime;

namespace Wayd.Common.Domain.Interfaces;

public interface ISoftDelete
{
    bool IsDeleted { get; set; }

    Instant? Deleted { get; set; }

    string? DeletedBy { get; set; }
}
