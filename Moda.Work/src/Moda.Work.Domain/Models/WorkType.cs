using CSharpFunctionalExtensions;

namespace Moda.Work.Domain.Models;

public class WorkType : BaseAuditableEntity<Guid>, IAggregateRoot, IActivatable
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
    public string Name { get; } = null!;

    /// <summary>
    /// The description of the work type.
    /// </summary>
    public string? Description { get; private set; }

    public bool IsActive { get; private set; } = true;

    public Result Activate()
    {
        throw new NotImplementedException();
    }

    public Result Deactivate()
    {
        throw new NotImplementedException();
    }
}
