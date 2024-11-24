using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Extensions;
using Moda.Common.Models;
using Moda.Work.Domain.Interfaces;
using NodaTime;

namespace Moda.Work.Domain.Models;

/// <summary>A workspace is a container for work items.</summary>
/// <seealso cref="Moda.Common.Domain.Data.BaseSoftDeletableEntity&lt;System.Guid&gt;" />
/// <seealso cref="Moda.Common.Domain.Interfaces.IActivatable&lt;Moda.Work.Domain.Models.WorkspaceActivatableArgs, NodaTime.Instant&gt;" />
public sealed class Workspace : BaseSoftDeletableEntity<Guid>, IActivatable<WorkspaceActivatableArgs, Instant>, HasWorkspaceIdAndKey
{
    private WorkspaceKey _key = null!;
    private string _name = null!;
    private string? _description;
    private string? _externalViewWorkItemUrlTemplate;
    private readonly List<WorkItem> _workItems = [];

    private Workspace() { }

    private Workspace(WorkspaceKey key, string name, string? description, Ownership ownership, Guid? externalId, Guid workProcessId, string? externalViewWorkItemUrlTemplate)
    {
        Key = key;
        Name = name;
        Description = description;
        Ownership = ownership;
        WorkProcessId = workProcessId;

        if (ownership is Ownership.Managed)
        {
            if (!externalId.HasValue)
                throw new ArgumentException("The external identifier is required when the ownership is managed.", nameof(externalId));

            ExternalId = externalId.Value;
            ExternalViewWorkItemUrlTemplate = null;
        }
    }

    /// <summary>A unique key to identify the workspace.</summary>
    /// <value>The key.</value>
    public WorkspaceKey Key
    {
        get => _key;
        private set => _key = Guard.Against.Null(value, nameof(Key));
    }

    /// <summary>
    /// The name of the workspace.
    /// </summary>
    public string Name
    {
        get => _name;
        private set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>
    /// The description of the workspace.
    /// </summary>
    public string? Description
    {
        get => _description;
        private set => _description = value.NullIfWhiteSpacePlusTrim();
    }

    /// <summary>
    /// Indicates whether the workspace is owned by Moda or a third party system.  This value should not change.
    /// </summary>
    public Ownership Ownership { get; private init; }

    /// <summary>Gets the external identifier. The value is required when Ownership is managed; otherwise it's null.</summary>
    /// <value>The external identifier.</value>
    public Guid? ExternalId { get; private init; }

    /// <summary>
    /// The foreign key for the work process.
    /// </summary>
    public Guid WorkProcessId { get; private set; }

    /// <summary>
    /// The work process assigned to the workspace.
    /// </summary>
    public WorkProcess? WorkProcess { get; private set; }

    /// <summary>
    /// A url template for external work items.  This template plus the work item external id will create a url to view the work item in the external system.
    /// </summary>
    public string? ExternalViewWorkItemUrlTemplate 
    { 
        get => _externalViewWorkItemUrlTemplate; 
        private set => _externalViewWorkItemUrlTemplate = value.NullIfWhiteSpacePlusTrim(); 
    }

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
    /// Creates a work item within the workspace.
    /// </summary>
    /// <param name="workItem"></param>
    /// <returns></returns>
    public Result AddWorkItem(WorkItem workItem)
    {
        if (!IsActive)
            return Result.Failure("Unable to create work items within an inactive work space.");

        // TODO: validate unique key, currently handled by the database

        _workItems.Add(workItem);
        return Result.Success();
    }

    /// <summary>
    /// Removes work items from the workspace.
    /// </summary>
    /// <param name="workItemIds"></param>
    /// <returns></returns>
    public Result RemoveWorkItems(IEnumerable<Guid> workItemIds)
    {
        if (!IsActive)
            return Result.Failure("Unable to remove work items in an inactive work space.");

        foreach (var workItemId in workItemIds)
        {
            var workItem = _workItems.FirstOrDefault(x => x.Id == workItemId);
            if (workItem != null)
                _workItems.Remove(workItem);
        }
        
        return Result.Success();
    }

    /// <summary>
    /// The process for updating the workspace properties.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    public Result Update(string name, string? description, Instant timestamp)
    {
        Name = name.Trim();
        Description = description?.Trim();

        AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));

        return Result.Success();
    }

    public Result SetExternalViewWorkItemUrlTemplate(string? externalWorkItemUrlTemplate, Instant timestamp)
    {
        if (Ownership is not Ownership.Managed)
            return Result.Failure($"Unable to set the external view work item url template for an {Ownership.GetDisplayName()} workspace.");

        // TODO: validate the template

        ExternalViewWorkItemUrlTemplate = externalWorkItemUrlTemplate;

        AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));

        return Result.Success();
    }

    /// <summary>
    /// The process for changing the work process assigned to the workspace.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    //public Result ChangeWorkspaceProcess(WorkProcess workProcess)
    //{
    //    if (workProcess.Ownership != Ownership)
    //        return Result.Failure($"Unable to assign the work process because the ownership does not match the workspace ownership.");

    //    if (!WorkItems.Any())
    //    {
    //        WorkProcessId = workProcess.Id;
    //        return Result.Success();
    //    }

    //    // TODO what if the work process doesn't cover all of the work types for existing work items
    //    //
    //    throw new NotImplementedException();
    //}

    /// <summary>The process for activating a workspace.</summary>
    /// <param name="args">The arguments.</param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public Result Activate(WorkspaceActivatableArgs args)
    {
        if (WorkProcessId != args.WorkProcess.Id)
            return Result.Failure($"Unable to activate the workspace because the work process does not match the workspace work process.");

        if (!args.WorkProcess.IsActive)
            return Result.Failure($"Unable to activate the workspace because the work process is not active.");

        if (!IsActive)
        {
            IsActive = true;
            AddDomainEvent(EntityActivatedEvent.WithEntity(this, args.Timestamp));
        }

        return Result.Success();
    }

    /// <summary>The process for deactivating a workspace.</summary>
    /// <param name="args">The arguments.</param>
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

    /// <summary>Creates an owned Workspace.</summary>
    /// <param name="key">The key.</param>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <param name="workProcessId">The work process.</param>
    /// <param name="timestamp">The timestamp.</param>
    /// <returns></returns>
    public static Workspace Create(WorkspaceKey key, string name, string? description, Guid workProcessId, Instant timestamp)
    {
        var workspace = new Workspace(key, name, description, Ownership.Owned, null, workProcessId, null);

        workspace.AddDomainEvent(EntityCreatedEvent.WithEntity(workspace, timestamp));
        return workspace;
    }

    /// <summary>Creates a managed Workspace linked to a external id.</summary>
    /// <param name="key">The key.</param>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <param name="externalId">The external identifier.</param>
    /// <param name="workProcess">The work process.</param>
    /// <param name="externalViewWorkItemUrlTemplate">The external view work item URL template.</param>
    /// <param name="timestamp">The timestamp.</param>
    /// <returns></returns>
    public static Workspace CreateExternal(WorkspaceKey key, string name, string? description, Guid externalId, Guid workProcessId, string? externalViewWorkItemUrlTemplate, Instant timestamp)
    {
        var workspace = new Workspace(key, name, description, Ownership.Managed, externalId, workProcessId, externalViewWorkItemUrlTemplate);

        workspace.AddDomainEvent(EntityCreatedEvent.WithEntity(workspace, timestamp));
        return workspace;
    }
}
