using FluentAssertions.Extensions;
using Moda.Common.Domain.Enums.Work;
using Moda.Tests.Shared.Data;
using Moda.Work.Domain.Models;
using NodaTime;
using NodaTime.Extensions;

namespace Moda.Work.Domain.Tests.Data;
public class WorkItemFaker : PrivateConstructorFaker<WorkItem>
{
    public WorkItemFaker(Guid? workspaceId = null)
    {
        var workType = new WorkTypeFaker().Generate();
        var workStatus = new WorkStatusFaker().Generate();

        var workspaceIdValue = workspaceId ?? FakerHub.Random.Guid();
        var category = FakerHub.Random.Enum<WorkStatusCategory>();

        Instant created = FakerHub.Date.Past().AsUtc().ToInstant();
        Instant? activatedTimestamp = null;
        Instant? doneTimestamp = null;

        var randomDays = FakerHub.Random.Int(0, 5);

        if (category is WorkStatusCategory.Active)
        {
            activatedTimestamp = created.Plus(Duration.FromDays(randomDays));
        }
        else if (category is WorkStatusCategory.Done)
        {
            activatedTimestamp = created.Plus(Duration.FromDays(randomDays));
            doneTimestamp = activatedTimestamp.Value.Plus(Duration.FromDays(FakerHub.Random.Int(1, 10)));
        }
        else if (category is WorkStatusCategory.Removed)
        {
            if (randomDays > 0)
            {
                activatedTimestamp = created.Plus(Duration.FromDays(randomDays));
                doneTimestamp = activatedTimestamp.Value.Plus(Duration.FromDays(FakerHub.Random.Int(1, 10)));
            }
            else
            {
                activatedTimestamp = doneTimestamp = created.Plus(Duration.FromDays(FakerHub.Random.Int(1, 10)));
            }            
        }

        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.WorkspaceId, f => workspaceIdValue);
        RuleFor(x => x.ExternalId, f => f.Random.Int(1, 10000));
        RuleFor(x => x.Title, f => f.Random.String2(10));
        RuleFor(x => x.TypeId, workType.Id);
        RuleFor(x => x.Type, workType);
        RuleFor(x => x.StatusId, workStatus.Id);
        RuleFor(x => x.Status, workStatus);
        RuleFor(x => x.StatusCategory, category);

        RuleFor(x => x.ParentId, f => null);
        RuleFor(x => x.Parent, f => null);
        RuleFor(x => x.Priority, f => f.Random.Int(1, 4));
        RuleFor(x => x.StackRank, f => f.Random.Double(1000, 100000));

        RuleFor(x => x.ProjectId, f => null);
        RuleFor(x => x.ParentProjectId, f => null);

        RuleFor(x => x.IterationId, f => null);

        RuleFor(x => x.Created, created);
        RuleFor(x => x.CreatedById, f => f.Random.Guid());
        RuleFor(x => x.LastModified, f => f.Date.Recent().AsUtc().ToInstant());
        RuleFor(x => x.LastModifiedById, f => f.Random.Guid());
        RuleFor(x => x.AssignedToId, f => f.Random.Guid());

        RuleFor(x => x.ActivatedTimestamp, activatedTimestamp);
        RuleFor(x => x.DoneTimestamp, doneTimestamp);
    }
}

public static class WorkItemFakerExtensions
{
    public static WorkItemFaker WithData(this WorkItemFaker faker, Guid? workspaceId = null, string? title = null, WorkType? type = null, WorkStatus? status = null, WorkStatusCategory? statusCategory = null, Guid? parentId = null, Instant? created = null, Guid? createdById = null, Instant? lastModified = null, Guid? lastModifiedById = null, Guid? assignedToId = null, int? priority = null, double? stackRank = null, Guid? projectId = null, Guid? parentProjectId = null, Guid? iterationId = null, Instant? activatedTimestamp = null, Instant? doneTimestamp = null)
    {
        if (workspaceId.HasValue) { faker.RuleFor(x => x.WorkspaceId, workspaceId.Value); }
        if (!string.IsNullOrWhiteSpace(title)) { faker.RuleFor(x => x.Title, title); }
        if (type != null) { faker.RuleFor(x => x.TypeId, type.Id); faker.RuleFor(x => x.Type, type); }
        if (status != null) { faker.RuleFor(x => x.StatusId, status.Id); faker.RuleFor(x => x.Status, status); }
        if (statusCategory.HasValue) { faker.RuleFor(x => x.StatusCategory, statusCategory.Value); }
        if (parentId.HasValue) { faker.RuleFor(x => x.ParentId, parentId.Value); }
        if (created.HasValue) { faker.RuleFor(x => x.Created, created.Value); }
        if (createdById.HasValue) { faker.RuleFor(x => x.CreatedById, createdById.Value); }
        if (lastModified.HasValue) { faker.RuleFor(x => x.LastModified, lastModified.Value); }
        if (lastModifiedById.HasValue) { faker.RuleFor(x => x.LastModifiedById, lastModifiedById.Value); }
        if (assignedToId.HasValue) { faker.RuleFor(x => x.AssignedToId, assignedToId.Value); }
        if (priority.HasValue) { faker.RuleFor(x => x.Priority, priority.Value); }
        if (stackRank.HasValue) { faker.RuleFor(x => x.StackRank, stackRank.Value); }
        if (projectId.HasValue) { faker.RuleFor(x => x.ProjectId, projectId.Value); }
        if (parentProjectId.HasValue) { faker.RuleFor(x => x.ParentProjectId, parentProjectId.Value); }
        if (iterationId.HasValue) { faker.RuleFor(x => x.IterationId, iterationId.Value); }
        if (activatedTimestamp.HasValue) { faker.RuleFor(x => x.ActivatedTimestamp, activatedTimestamp.Value); }
        if (doneTimestamp.HasValue) { faker.RuleFor(x => x.DoneTimestamp, doneTimestamp.Value); }

        return faker;
    }

    /// <summary>
    /// Creates a work item that hasn't started yet (Proposed status category with no activated or done timestamps).
    /// </summary>
    public static WorkItemFaker WithProposedState(this WorkItemFaker faker)
    {
        var workStatus = new WorkStatusFaker().WithData(name: "To Do").Generate();

        faker.RuleFor(x => x.StatusCategory, WorkStatusCategory.Proposed);
        faker.RuleFor(x => x.StatusId, workStatus.Id);
        faker.RuleFor(x => x.Status, workStatus);
        faker.RuleFor(x => x.ActivatedTimestamp, (Instant?)null);
        faker.RuleFor(x => x.DoneTimestamp, (Instant?)null);

        return faker;
    }

    /// <summary>
    /// Creates a work item that is currently in progress (Active status category with activated timestamp set and no done timestamp).
    /// </summary>
    /// <param name="faker"></param>
    /// <returns></returns>
    public static WorkItemFaker WithActiveState(this WorkItemFaker faker)
    {
        var workStatus = new WorkStatusFaker().WithData(name: "In Progress").Generate();
        var created = faker.Generate().Created;
        var activated = created.Plus(Duration.FromDays(1));

        faker.RuleFor(x => x.StatusCategory, WorkStatusCategory.Active);
        faker.RuleFor(x => x.StatusId, workStatus.Id);
        faker.RuleFor(x => x.Status, workStatus);
        faker.RuleFor(x => x.ActivatedTimestamp, activated);
        faker.RuleFor(x => x.DoneTimestamp, (Instant?)null);

        return faker;
    }

    /// <summary>
    /// Creates a work item that has been completed (Done status category with both activated and done timestamps set).
    /// </summary>
    /// <param name="faker"></param>
    /// <returns></returns>
    public static WorkItemFaker WithDoneState(this WorkItemFaker faker)
    {
        var workStatus = new WorkStatusFaker().WithData(name: "Done").Generate();
        var created = faker.Generate().Created;
        var activated = created.Plus(Duration.FromDays(1));
        var done = activated.Plus(Duration.FromDays(3));

        faker.RuleFor(x => x.StatusCategory, WorkStatusCategory.Done);
        faker.RuleFor(x => x.StatusId, workStatus.Id);
        faker.RuleFor(x => x.Status, workStatus);
        faker.RuleFor(x => x.ActivatedTimestamp, activated);
        faker.RuleFor(x => x.DoneTimestamp, done);

        return faker;
    }
}
