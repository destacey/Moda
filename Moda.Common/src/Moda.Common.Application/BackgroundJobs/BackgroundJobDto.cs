using System.Reflection;

namespace Moda.Common.Application.BackgroundJobs;
public record BackgroundJobDto
{
    public string? Name { get; set; }
    public Type? Type { get; set; }
    public MethodInfo? Method { get; set; }
    public IReadOnlyList<object> Args { get; set; } = new List<object>();
    public bool InProcessingState { get; set; }
    public DateTime? StartedAt { get; set; }
}
