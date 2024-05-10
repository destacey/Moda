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
        if (IsActive)
        {
            IsActive = false;
            AddDomainEvent(EntityDeactivatedEvent.WithEntity(this, timestamp));
        }

        return Result.Success();
    }

    internal Result Update(Guid workflowId, Instant timestamp)
    {
        try
        {
            WorkflowId = workflowId;

            AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    internal static WorkProcessScheme CreateExternal(Guid workProcessId, int workTypeId, bool isActive)
    {
        var scheme = new WorkProcessScheme (workProcessId, workTypeId, null);

        // external work process schemes do not have to be active when they are first created in Moda
        if (scheme.IsActive != isActive)
            scheme.IsActive = isActive;

        return scheme;
    }
}
