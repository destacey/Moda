using CSharpFunctionalExtensions;
using NodaTime;

namespace Moda.Work.Domain.Models;

public sealed class WorkflowScheme : BaseAuditableEntity<Guid>, IActivatable
{
    private WorkflowScheme() { }
    internal WorkflowScheme(Guid workflowId, int workStatusId, WorkStatusCategory workStatusCategory, int order)
    {
        WorkflowId = workflowId;
        WorkStatusId = workStatusId;
        WorkStatusCategory = workStatusCategory;
        Order = order;
    }

    public Guid WorkflowId { get; }
    public int WorkStatusId { get; }
    public WorkStatus? WorkStatus { get; private set; }
    public WorkStatusCategory WorkStatusCategory { get; private set; }
    public int Order { get; private set; }

    public bool IsActive { get; private set; } = true;

    public Result Activate(Instant timestamp)
    {
        if (!IsActive)
        {
            IsActive = true;
            AddDomainEvent(EntityActivatedEvent.WithEntity(this, timestamp));
        }

        return Result.Success();
    }

    public Result Deactivate(Instant timestamp)
    {
        if (IsActive)
        {
            IsActive = false;
            AddDomainEvent(EntityDeactivatedEvent.WithEntity(this, timestamp));
        }

        return Result.Success();
    }

    public Result Update(WorkStatusCategory workStatusCategory, int order, Instant timestamp)
    {
        try
        {
            WorkStatusCategory = workStatusCategory;
            Order = order;

            AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    public static WorkflowScheme Create(Guid workflowId, int workStatusId, WorkStatusCategory workStatusCategory, int order)
    {
        // TODO move this to be managed by the Workflow as the aggregate root
        return new(workflowId, workStatusId, workStatusCategory, order);
    }
}
