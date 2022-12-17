using CSharpFunctionalExtensions;
using NodaTime;

namespace Moda.Work.Domain.Models;

/// <summary>
/// 
/// </summary>
/// <seealso cref="Moda.Common.Domain.Data.BaseAuditableEntity&lt;System.Guid&gt;" />
/// <seealso cref="Moda.Common.Domain.Interfaces.IActivatable&lt;NodaTime.Instant&gt;" />
public sealed class Workflow : BaseAuditableEntity<Guid>, IActivatable<Instant>
{
    private readonly List<WorkflowConfiguration> _configurations = new();

    private Workflow() { }

    public Workflow(string name, string? description, Ownership ownership)
    {
        Name = name.Trim();
        Description = description?.Trim();
        Ownership = ownership;
    }

    /// <summary>
    /// The name of the workflow.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// The description of the workflow.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Indicates whether the workflow is owned by Moda or a third party system.  This value should not change.
    /// </summary>
    public Ownership Ownership { get; }

    /// <summary>
    /// Indicates whether the workflow is active or not.  Only active workflows can be assigned 
    /// to work process configurations.  The default is false and the user should activate it after the configuration is complete.
    /// </summary>
    public bool IsActive { get; private set; } = false;

    public IReadOnlyCollection<WorkflowConfiguration> Configurations => _configurations.AsReadOnly();

    /// <summary>
    /// The process for activating a workflow.
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public Result Activate(Instant timestamp)
    {
        if (!IsActive)
        {
            // TODO is there logic that would prevent activation?
            IsActive = true;
            AddDomainEvent(EntityActivatedEvent.WithEntity(this, timestamp));
        }

        return Result.Success();
    }

    /// <summary>
    /// The process for deactivating a workflow.
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public Result Deactivate(Instant timestamp)
    {
        if (IsActive)
        {
            // TODO is there logic that would prevent deactivation?
            IsActive = false;
            AddDomainEvent(EntityDeactivatedEvent.WithEntity(this, timestamp));
        }

        return Result.Success();
    }
}
