using Moda.Common.Domain.Models.ProjectPortfolioManagement;
using Moda.Common.Models;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Models;
using Moda.Tests.Shared;
using Moda.Tests.Shared.Data;
using NodaTime;
using TaskStatus = Moda.ProjectPortfolioManagement.Domain.Enums.TaskStatus;

namespace Moda.ProjectPortfolioManagement.Domain.Tests.Data;

public sealed class ProjectTaskFaker : PrivateConstructorFaker<ProjectTask>
{
    public ProjectTaskFaker()
    {
        var projectKey = new ProjectKey("TEST");
        var number = FakerHub.Random.Int(1, 10000);
        var taskKey = new ProjectTaskKey(projectKey, number);

        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.Key, f => taskKey);
        RuleFor(x => x.Number, f => number);
        RuleFor(x => x.ProjectId, f => f.Random.Guid());
        RuleFor(x => x.Name, f => f.Lorem.Sentence(3));
        RuleFor(x => x.Description, f => f.Lorem.Paragraph());
        RuleFor(x => x.Type, f => ProjectTaskType.Task);
        RuleFor(x => x.Status, f => TaskStatus.NotStarted);
        RuleFor(x => x.Priority, f => f.PickRandom(new TaskPriority[] { TaskPriority.Low, TaskPriority.Medium, TaskPriority.High, TaskPriority.High }));
        RuleFor(x => x.Progress, f => Progress.NotStarted());
        RuleFor(x => x.Order, f => f.Random.Int(1, 10));
        RuleFor(x => x.ParentId, f => null); // No parent by default
        RuleFor(x => x.PlannedDateRange, f => null);
        RuleFor(x => x.EstimatedEffortHours, f => f.Random.Decimal(1, 100));
    }
}

public static class ProjectTaskFakerExtensions
{
    public static ProjectTaskFaker WithData(
        this ProjectTaskFaker faker,
        Guid? id = null,
        ProjectTaskKey? key = null,
        Guid? projectId = null,
        string? name = null,
        string? description = null,
        ProjectTaskType? type = null,
        TaskStatus? status = null,
        TaskPriority? priority = null,
        Progress? progress = null,
        int? order = null,
        Guid? parentId = null,
        FlexibleDateRange? plannedDateRange = null,
        LocalDate? plannedDate = null,
        decimal? estimatedEffortHours = null)
    {
        if (id.HasValue) { faker.RuleFor(x => x.Id, id.Value); }
        if (key is not null) 
        { 
            faker.RuleFor(x => x.Key, key);
            faker.RuleFor(x => x.Number, key.TaskNumber);
        }
        if (projectId.HasValue) { faker.RuleFor(x => x.ProjectId, projectId.Value); }
        if (!string.IsNullOrWhiteSpace(name)) { faker.RuleFor(x => x.Name, name); }
        if (!string.IsNullOrWhiteSpace(description)) { faker.RuleFor(x => x.Description, description); }
        if (type.HasValue) { faker.RuleFor(x => x.Type, type.Value); }
        if (status.HasValue) { faker.RuleFor(x => x.Status, status.Value); }
        if (priority.HasValue) { faker.RuleFor(x => x.Priority, priority); }
        if (progress is not null) { faker.RuleFor(x => x.Progress, progress); }
        if (order.HasValue) { faker.RuleFor(x => x.Order, order.Value); }
        if (parentId.HasValue) { faker.RuleFor(x => x.ParentId, parentId); }
        if (plannedDateRange is not null) { faker.RuleFor(x => x.PlannedDateRange, plannedDateRange); }
        if (plannedDate.HasValue) { faker.RuleFor(x => x.PlannedDate, plannedDate); }
        if (estimatedEffortHours.HasValue) { faker.RuleFor(x => x.EstimatedEffortHours, estimatedEffortHours); }

        return faker;
    }

    /// <summary>
    /// Generates a task in NotStarted status with planned dates.
    /// </summary>
    public static ProjectTask AsNotStarted(this ProjectTaskFaker faker, TestingDateTimeProvider dateTimeProvider, Guid projectId, ProjectKey projectKey)
    {
        var now = dateTimeProvider.Today;
        var startDate = now.PlusDays(5);
        var endDate = startDate.PlusDays(10);
        var progress = new Progress(Decimal.Zero);

        return faker.WithData(
            projectId: projectId,
            key: new ProjectTaskKey(projectKey, new Random().Next(1, 999)),
            status: TaskStatus.NotStarted,
            progress: progress,
            plannedDateRange: new FlexibleDateRange(startDate, endDate)
        ).Generate();
    }

    /// <summary>
    /// Generates a task in InProgress status.
    /// </summary>
    public static ProjectTask AsInProgress(this ProjectTaskFaker faker, TestingDateTimeProvider dateTimeProvider, Guid projectId, ProjectKey projectKey)
    {
        var now = dateTimeProvider.Today;
        var startDate = now.PlusDays(-5);
        var endDate = now.PlusDays(10);
        var progress = new Progress(0.25m);

        return faker.WithData(
            projectId: projectId,
            key: new ProjectTaskKey(projectKey, new Random().Next(1, 999)),
            status: TaskStatus.InProgress,
            progress: progress,
            plannedDateRange: new FlexibleDateRange(startDate, endDate)
        ).Generate();
    }

    /// <summary>
    /// Generates a task in Completed status.
    /// </summary>
    public static ProjectTask AsCompleted(this ProjectTaskFaker faker, TestingDateTimeProvider dateTimeProvider, Guid projectId, ProjectKey projectKey)
    {
        var now = dateTimeProvider.Today;
        var startDate = now.PlusDays(-15);
        var endDate = now.PlusDays(-5);
        var progress = Progress.Completed();

        return faker.WithData(
            projectId: projectId,
            key: new ProjectTaskKey(projectKey, new Random().Next(1, 999)),
            status: TaskStatus.Completed,
            progress: progress,
            plannedDateRange: new FlexibleDateRange(startDate, endDate)
        ).Generate();
    }

    /// <summary>
    /// Generates a milestone task with a planned date.
    /// </summary>
    public static ProjectTask AsMilestone(this ProjectTaskFaker faker, TestingDateTimeProvider dateTimeProvider, Guid projectId, ProjectKey projectKey)
    {
        var now = dateTimeProvider.Today;
        var milestoneDate = now.PlusDays(30);

        return faker.WithData(
            projectId: projectId,
            key: new ProjectTaskKey(projectKey, new Random().Next(1, 999)),
            type: ProjectTaskType.Milestone,
            plannedDate: milestoneDate
        ).Generate();
    }
}
