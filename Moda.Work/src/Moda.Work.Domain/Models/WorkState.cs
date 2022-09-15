using CSharpFunctionalExtensions;

namespace Moda.Work.Domain.Models;

public class WorkState : BaseAuditableEntity<Guid>, IAggregateRoot, IActivatable
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
    public string Name { get; private set; } = null!;

    /// <summary>
    /// The description of the work state.
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
