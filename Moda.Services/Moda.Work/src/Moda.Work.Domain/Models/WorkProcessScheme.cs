using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using NodaTime;

namespace Moda.Work.Domain.Models;

public sealed class WorkProcessScheme : BaseAuditableEntity<Guid>, IActivatable
{
    private WorkProcessScheme() { }

    internal WorkProcessScheme(Guid workProcessId, int workTypeId, Guid? workflowId)
    {
        WorkProcessId = workProcessId;
        WorkTypeId = workTypeId;
        WorkflowId = workflowId;
    }
    internal WorkProcessScheme(WorkProcess workProcess, int workTypeId, Guid? workflowId)
    {
        WorkProcess = workProcess;
        WorkTypeId = workTypeId;
        WorkflowId = workflowId;
    }

    public Guid WorkProcessId { get; }
    public WorkProcess? WorkProcess { get; private set; }
    public int WorkTypeId { get; }
    public WorkType? WorkType { get; private set; }
    public Guid? WorkflowId { get; private set; }
    public Workflow? Workflow { get; private set; }
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// The process for activating a work process scheme.
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public Result Activate(Instant timestamp)
    {
        if (!IsActive)
        {
            IsActive = true;
            AddDomainEvent(EntityActivatedEvent.WithEntity(this, timestamp));
        }

        return Result.Success();
    }

    /// <summary>
    /// The process for deactivating a work process scheme.
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public Result Deactivate(Instant timestamp)
    {
        Guard.Against.Null(WorkProcess, nameof(WorkProcess));

        if (IsActive)
        {
            IsActive = false;
            AddDomainEvent(EntityDeactivatedEvent.WithEntity(this, timestamp));
        }

        return Result.Success();
    }

    internal Result ChangeWorkflow(Guid workflowId)
    {
        Guard.Against.Default(workflowId, nameof(workflowId));

        WorkflowId = workflowId;

        return Result.Success();
    }

    internal static WorkProcessScheme CreateExternal(Guid workProcessId, int workTypeId, Guid workflowId, bool isActive)
    {
        Guard.Against.Default(workProcessId, nameof(workProcessId));
        Guard.Against.Default(workflowId, nameof(workflowId));

        var scheme = new WorkProcessScheme(workProcessId, workTypeId, workflowId);

        // external work process schemes do not have to be active when they are first created in Moda
        if (scheme.IsActive != isActive)
            scheme.IsActive = isActive;

        return scheme;
    }

    /// <summary>
    /// Used when creating a new external work process scheme and work process together.  EF will set the WorkProcessId when the work process is saved.
    /// </summary>
    /// <param name="workProcess"></param>
    /// <param name="workTypeId"></param>
    /// <param name="workflowId"></param>
    /// <param name="isActive"></param>
    /// <returns></returns>
    internal static WorkProcessScheme CreateExternal(WorkProcess workProcess, int workTypeId, Guid workflowId, bool isActive)
    {
        var scheme = new WorkProcessScheme(workProcess, workTypeId, workflowId);

        // external work process schemes do not have to be active when they are first created in Moda
        if (scheme.IsActive != isActive)
            scheme.IsActive = isActive;

        return scheme;
    }
}
