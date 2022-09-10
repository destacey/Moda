namespace Moda.Work.Domain.Models;

public class WorkType : BaseAuditableEntity<Guid>
{
    public WorkType(string name, string? description)
    {
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
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
