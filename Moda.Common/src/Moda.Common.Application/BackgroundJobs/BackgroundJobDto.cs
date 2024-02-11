namespace Moda.Common.Application.BackgroundJobs;
public record BackgroundJobDto
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public bool InProcessingState { get; set; }
    public Instant? StartedAt { get; set; }
}
