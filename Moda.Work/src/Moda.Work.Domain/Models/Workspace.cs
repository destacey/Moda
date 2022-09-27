using CSharpFunctionalExtensions;

namespace Moda.Work.Domain.Models;

/// <summary>
/// A workspace is a container for work items.
/// </summary>
public sealed class Workspace : BaseAuditableEntity<Guid>, IAggregateRoot, IActivatable
{
    private readonly List<WorkItem> _workItems = new();

    private Workspace() { }

    public Workspace(string name, string? description, Ownership ownership, Guid workProcessId)
    {
        Name = name.Trim();
        Description = description?.Trim();
        Ownership = ownership;
        WorkProcessId = workProcessId;
    }

    /// <summary>
    /// The name of the workspace.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// The description of the workspace.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Indicates whether the workspace is owned by Moda or a third party system.  This value should not change.
    /// </summary>
    public Ownership Ownership { get; }

    ///// <summary>
    ///// The foreign key for the organization that ownes the workspace.  This value should not change.
    ///// TODO should this be able to change over time?  Add functionality to transfer organizational ownership.
    ///// </summary>
    //public Guid OrganizationId { get; }

    /// <summary>
    /// The foreign key for the work process.
    /// </summary>
    public Guid WorkProcessId { get; private set; }

    /// <summary>
    /// The work process assigned to the project.  The work process provides the work types, workflows, and configuration
    /// for the workspace.
    /// </summary>
    public WorkProcess WorkProcess { get; private set; } = null!;

    /// <summary>
    /// Indicates whether the workspace is active or not.  Inactive workspaces will be locked.  This means that
    /// users won't be able to add or updated work items.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// A collection of work items in the workspace.
    /// </summary>
    public IReadOnlyCollection<WorkItem> WorkItems => _workItems.AsReadOnly();

    /// <summary>
    /// The process for adding a work item to the workspace.
    /// </summary>
    /// <param name="workItem"></param>
    /// <returns></returns>
    public Result AddWorkItem(WorkItem workItem)
    {
        if (!IsActive)
            return Result.Failure("Unable to add work items to an inactive work space.");

        _workItems.Add(workItem);
        return Result.Success();
    }

    /// <summary>
    /// The process for updating the workspace properties.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    public Result Update(string name, string? description)
    {
        Name = name.Trim();
        Description = description?.Trim();

        return Result.Success();
    }

    /// <summary>
    /// The process for changing the work process assigned to the workspace.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public Result ChangeWorkspaceProcess(WorkProcess workProcess)
    {
        if (workProcess.Ownership != Ownership)
            return Result.Failure($"Unable to assign the work process because the ownership does not match the workspace ownership.");

        if (!WorkItems.Any())
        {
            WorkProcessId = workProcess.Id;
            WorkProcess = workProcess;
            return Result.Success();
        }

        // TODO what if the work process doesn't cover all of the work types for existing work items
        //
        throw new NotImplementedException();
    }

    /// <summary>
    /// The process for activating a workspace.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public Result Activate()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// The process for deactivating a workspace. 
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public Result Deactivate()
    {
        // TODO what is need to deactive a managed work process

        if (IsActive)
            IsActive = false;

        return Result.Success();
    }
}
