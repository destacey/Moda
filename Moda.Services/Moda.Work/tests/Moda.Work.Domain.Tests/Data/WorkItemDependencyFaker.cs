using Moda.Tests.Shared.Data;
using Moda.Tests.Shared.Extensions;
using Moda.Work.Domain.Models;
using NodaTime;
using NodaTime.Extensions;

namespace Moda.Work.Domain.Tests.Data;
public class WorkItemDependencyFaker : PrivateConstructorFaker<WorkItemDependency>
{
    public WorkItemDependencyFaker(Instant now)
    {
        var workItemFaker = new WorkItemFaker();
        var sourceWorkItem = workItemFaker.WithProposedState().Generate();
        var targetWorkItem = workItemFaker.WithProposedState().Generate();

        RuleFor(x => x.SourceId, sourceWorkItem.Id);
        RuleFor(x => x.Source, sourceWorkItem);
        RuleFor(x => x.SourceStatusCategory, sourceWorkItem.StatusCategory);
        RuleFor(x => x.TargetId, targetWorkItem.Id);
        RuleFor(x => x.Target, targetWorkItem);
        RuleFor(x => x.TargetStatusCategory, targetWorkItem.StatusCategory);
        RuleFor(x => x.SourcePlannedOn, f => null);
        RuleFor(x => x.TargetPlannedOn, f => null);
        RuleFor(x => x.CreatedOn, f => f.Date.Past().ToInstant());
        RuleFor(x => x.CreatedById, f => f.Random.Guid());
        RuleFor(x => x.Comment, f => f.Lorem.Sentence());


        // Call CalculateStateAndHealth() after generation
        this.FinishWith("CalculateStateAndHealth", now);
    }
}

public static class WorkItemDependencyFakerExtensions
{
    public static WorkItemDependencyFaker WithData(this WorkItemDependencyFaker faker, WorkItem? source = null, WorkItem? target = null, Instant? sourcePlannedOn = null, Instant? targetPlannedOn = null, Instant? createdOn = null, Guid? createdById = null, string? comment = null)
    {
        if (source != null) 
        { 
            faker.RuleFor(x => x.SourceId, source.Id); 
            faker.RuleFor(x => x.Source, source);
            faker.RuleFor(x => x.SourceStatusCategory, source.StatusCategory);
        }
        if (target != null) 
        { 
            faker.RuleFor(x => x.TargetId, target.Id); 
            faker.RuleFor(x => x.Target, target);
            faker.RuleFor(x => x.TargetStatusCategory, target.StatusCategory);
        }
        if (sourcePlannedOn.HasValue) { faker.RuleFor(x => x.SourcePlannedOn, sourcePlannedOn.Value); }
        if (targetPlannedOn.HasValue) { faker.RuleFor(x => x.TargetPlannedOn, targetPlannedOn.Value); }
        if (createdOn.HasValue) { faker.RuleFor(x => x.CreatedOn, createdOn.Value); }
        if (createdById.HasValue) { faker.RuleFor(x => x.CreatedById, createdById.Value); }
        if (comment != null) { faker.RuleFor(x => x.Comment, comment); }

        return faker;
    }
}
