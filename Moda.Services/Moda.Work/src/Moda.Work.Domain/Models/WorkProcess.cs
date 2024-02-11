using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Extensions;
using NodaTime;

namespace Moda.Work.Domain.Models;

/// <summary>
/// The work process defines a set of work process configurations that can be used
/// within a workspace. A work process can be used in many workspaces.
/// </summary>
/// <seealso cref="Moda.Common.Domain.Data.BaseAuditableEntity&lt;System.Guid&gt;" />
/// <seealso cref="Moda.Common.Domain.Interfaces.IActivatable" />
public sealed class WorkProcess : BaseAuditableEntity<Guid>, IActivatable
{
    private string _name = null!;
    private string? _description;

    private readonly List<WorkProcessScheme> _schemes = [];
    private readonly List<Workspace> _workspaces = [];

    private WorkProcess() { }

    private WorkProcess(string name, string? description, Ownership ownership, Guid? externalId)
    {
        Name = name;
        Description = description;
        Ownership = ownership;

        if (ownership is Ownership.Managed)
        {
            if (!externalId.HasValue)
                throw new ArgumentException("The external identifier is required when the ownership is managed.", nameof(externalId));

            ExternalId = externalId.Value;
        }
    }

    /// <summary>Gets the key.</summary>
    /// <value>The key.</value>
    public int Key { get; private init; }

    /// <summary>
    /// The name of the work process.
    /// </summary>
    public string Name
    {
        get => _name;
        private set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>
    /// The description of the work process.
    /// </summary>
    public string? Description
    {
        get => _description;
        private set => _description = value.NullIfWhiteSpacePlusTrim();
    }

    /// <summary>
    /// Indicates whether the work process is owned by Moda or a third party system.  This value should not change.
    /// </summary>
    /// <value>The ownership.</value>
    public Ownership Ownership { get; private init; }

    /// <summary>
    /// Gets the external identifier. The value is required when Ownership is managed; otherwise it's null.  For Azure DevOps, this is the process id.
    /// </summary>
    public Guid? ExternalId { get; private init; }

    /// <summary>
    /// Indicates whether the work process is active or not.  Only active work processes can be assigned
    /// to workspaces.  The default is false and the user should activate it after the schemes are complete.
    /// </summary>
    /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
    public bool IsActive { get; private set; } = false;

    public IReadOnlyCollection<WorkProcessScheme> Schemes => _schemes.AsReadOnly();

    public IReadOnlyCollection<Workspace> Workspaces => _workspaces.AsReadOnly();

    public Result Update(string name, string? description, Instant timestamp)
    {
        try
        {
            Name = name;
            Description = description;

            AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    /// <summary>
    /// The process for activating a work process.  A work process can only be activated if the configuration
    /// is valid.
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

            TryAddIntegrationStateChangedEvent(timestamp);
        }

        return Result.Success();
    }

    /// <summary>
    /// The process for deactivating a work process.  Only work processes without active assignments can be deactivated.
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public Result Deactivate(Instant timestamp)
    {
        // TODO what is need to deactive a managed work process

        if (IsActive)
        {
            if (Workspaces.Any(w => w.IsActive))
                return Result.Failure("Unable to deactive with active workspaces.");

            IsActive = false;
            AddDomainEvent(EntityDeactivatedEvent.WithEntity(this, timestamp));

            TryAddIntegrationStateChangedEvent(timestamp);
        }

        return Result.Success();
    }

    private void TryAddIntegrationStateChangedEvent(Instant timestamp)
    {
        if (Ownership is Ownership.Managed && ExternalId.HasValue)
        {
            AddDomainEvent(new IntegrationStateChangedEvent<Guid>(
                SystemContext.WorkWorkProcess,
                IntegrationState<Guid>.Create(Id, IsActive),
                timestamp));
        }
    }

    /// <summary>Creates an owned Work Process.</summary>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <param name="ownership">The ownership.</param>
    /// <param name="timestamp">The timestamp.</param>
    /// <returns></returns>
    public static WorkProcess Create(string name, string? description, Instant timestamp)
    {
        WorkProcess workProcess = new(name, description, Ownership.Owned, null);

        workProcess.AddDomainEvent(EntityCreatedEvent.WithEntity(workProcess, timestamp));
        return workProcess;
    }

    /// <summary>Creates a managed Work Process linked to a external id.</summary>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <param name="externalId">The external identifier.</param>
    /// <param name="timestamp">The timestamp.</param>
    /// <returns></returns>
    public static WorkProcess CreateExternal(string name, string? description, Guid externalId, Instant timestamp)
    {
        WorkProcess workProcess = new(name, description, Ownership.Managed, externalId);

        workProcess.AddDomainEvent(EntityCreatedEvent.WithEntity(workProcess, timestamp));
        return workProcess;
    }
}
