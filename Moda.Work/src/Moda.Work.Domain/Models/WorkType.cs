namespace Moda.Work.Domain.Models;

public class WorkType : BaseAuditableEntity<Guid>
{
    private WorkType() { }

    public WorkType(string name, string? description)
    {
        Name = name.Trim();
        Description = description?.Trim();
    }

    /// <summary>
    /// The name of the work type.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The description of the work type.
    /// </summary>
    public string? Description { get; }
}
