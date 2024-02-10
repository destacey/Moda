using NodaTime;

namespace Moda.Infrastructure.Auditing;

public class Trail : BaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public string? Type { get; set; }
    public string? SchemaName { get; set; }
    public string? TableName { get; set; }
    public Instant DateTime { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? AffectedColumns { get; set; }
    public string? PrimaryKey { get; set; }
    public string? CorrelationId { get; set; }
}