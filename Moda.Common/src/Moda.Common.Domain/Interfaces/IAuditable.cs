using NodaTime;

namespace Moda.Common.Domain.Interfaces;

public interface IAuditable
{
    public string? CreatedBy { get; set; }
    public Instant Created { get; set; }
    public string? LastModifiedBy { get; set; }
    public Instant LastModified { get; set; }
}
