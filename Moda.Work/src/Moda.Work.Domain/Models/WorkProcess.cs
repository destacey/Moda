using CSharpFunctionalExtensions;
using NodaTime;

namespace Moda.Work.Domain.Models;

/// <summary>
/// The work process defines a set of work process configurations that can be used 
/// within a workspace. A work process can be used in many workspaces.
/// </summary>
public sealed class WorkProcess : BaseAuditableEntity<Guid>, IAggregateRoot, IActivatable
{
    private readonly List<WorkProcessConfiguration> _configurations = new();
    private readonly List<Workspace> _workspaces = new();

    private WorkProcess() { }

    public WorkProcess(string name, string? description, Ownership ownership)
    {
        Name = name.Trim();
        Description = description?.Trim();
        Ownership = ownership;
    }

    /// <summary>
    /// The name of the work process.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// The description of the work process.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Indicates whether the work process is owned by Moda or a third party system.  This value should not change.
    /// </summary>
    public Ownership Ownership { get; }

    /// <summary>
    /// Indicates whether the work process is active or not.  Only active work processes can be assigned 
    /// to workspaces.  The default is false and the user should activate it after the configurations are complete.
    /// </summary>
    public bool IsActive { get; private set; } = false;

    public IReadOnlyCollection<WorkProcessConfiguration> Configurations => _configurations.AsReadOnly();

    public IReadOnlyCollection<Workspace> Workspaces => _workspaces.AsReadOnly();

    /// <summary>
    /// The process for activating a work process.  A work process can only be activated if the configuration
    /// is valid.
    /// </summary>
    /// <param name="activatedOn"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public Result Activate(Instant activatedOn)
    {
        if (!IsActive)
        {
            // TODO is there logic that would prevent activation?
            IsActive = true;
            AddDomainEvent(EntityActivatedEvent.WithEntity(this, activatedOn));
        }

        return Result.Success();
    }

    /// <summary>
    /// The process for deactivating a work process.  Only work processes without assignments can be deactivated.
    /// </summary>
    /// <param name="deactivatedOn"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public Result Deactivate(Instant deactivatedOn)
    {
        // TODO what is need to deactive a managed work process

        if (IsActive)
        {
            if (Workspaces.Any())
                return Result.Failure("Unable to deactive with assigned workspaces.");

            IsActive = false;
            AddDomainEvent(EntityDeactivatedEvent.WithEntity(this, deactivatedOn));
        }

        return Result.Success();
    }
}
