using CSharpFunctionalExtensions;

namespace Moda.Work.Domain.Models;

public class Workflow : BaseAuditableEntity<Guid>, IAggregateRoot, IActivatable
{
    private readonly List<WorkflowConfiguration> _configurations = new();

    public Workflow(string name, string? description)
    {
        Name = name.Trim();
        Description = description?.Trim();
    }

    private Workflow() { }

    /// <summary>
    /// The name of the workflow.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// The description of the workflow.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Indicates whether the workflow is active or not.  Only active workflows can be assigned 
    /// to work process configurations.  The default is false and the user should activate it after the configuration is complete.
    /// </summary>
    public bool IsActive { get; private set; } = false;

    public IReadOnlyCollection<WorkflowConfiguration> Configurations => _configurations.AsReadOnly();

    public Result Activate()
    {
        throw new NotImplementedException();
    }

    public Result Deactivate()
    {
        throw new NotImplementedException();
    }
}
