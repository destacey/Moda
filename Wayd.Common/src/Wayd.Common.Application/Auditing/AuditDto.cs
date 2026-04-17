namespace Wayd.Common.Application.Auditing;

public sealed record AuditDto
{
    public Guid Id { get; set; }
    public required string UserId { get; set; }
    public string? Type { get; set; }
    public string? TableName { get; set; }
    public Instant DateTime { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? AffectedColumns { get; set; }
    public string? PrimaryKey { get; set; }
}