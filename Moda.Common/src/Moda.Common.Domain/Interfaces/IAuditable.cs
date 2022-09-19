using NodaTime;

namespace Moda.Common.Domain.Interfaces;

public interface IAuditable
{
    public Guid CreatedBy { get; set; }
    public Instant Created { get; }
    public Guid LastModifiedBy { get; set; }
    public Instant LastModified { get; set; }
}
