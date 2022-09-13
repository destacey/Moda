namespace Moda.Work.Domain.Models;

public class WorkState : BaseAuditableEntity<Guid>
{
    private WorkState() { }

    public WorkState(string name, string? description)
    {
        Name = name.Trim();
        Description = description?.Trim();
    }

    /// <summary>
    /// The name of the work state.
    /// </summary>
    public string Name { get; } = null!;

    /// <summary>
    /// The description of the work state.
    /// </summary>
    public string? Description { get; }
}
