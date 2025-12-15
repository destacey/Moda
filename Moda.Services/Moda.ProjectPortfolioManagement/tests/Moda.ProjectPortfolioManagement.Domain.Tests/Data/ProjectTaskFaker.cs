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
        RuleFor(x => x.Id, f => f.Random.Guid());
        RuleFor(x => x.Key, f => f.Random.Int(1, 1000));
        RuleFor(x => x.TaskKey, f => new ProjectTaskKey(new ProjectKey(f.Random.AlphaNumeric(5).ToUpper()), f.Random.Int(1, 999)));
        RuleFor(x => x.ProjectId, f => f.Random.Guid());
        RuleFor(x => x.Name, f => f.Lorem.Sentence(3));
        RuleFor(x => x.Description, f => f.Lorem.Paragraph());
        RuleFor(x => x.Type, f => ProjectTaskType.Task);
        RuleFor(x => x.Status, f => TaskStatus.NotStarted);
        RuleFor(x => x.Priority, f => f.PickRandom(new TaskPriority?[] { null, TaskPriority.Low, TaskPriority.Medium, TaskPriority.High }));
        RuleFor(x => x.Order, f => f.Random.Int(1, 100));
        RuleFor(x => x.ParentId, f => null); // No parent by default
        RuleFor(x => x.TeamId, f => null);
        RuleFor(x => x.PlannedDateRange, f => null);
        RuleFor(x => x.ActualDateRange, f => null);
        RuleFor(x => x.PlannedDate, f => null);
        RuleFor(x => x.ActualDate, f => null);
        RuleFor(x => x.EstimatedEffortHours, f => f.Random.Decimal(1, 100));
        RuleFor(x => x.ActualEffortHours, f => null);
    }
}

public static class ProjectTaskFakerExtensions
{
    public static ProjectTaskFaker WithData(
        this ProjectTaskFaker faker,
        Guid? id = null,
        int? key = null,
        ProjectTaskKey? taskKey = null,
        Guid? projectId = null,
        string? name = null,
        string? description = null,
        ProjectTaskType? type = null,
        TaskStatus? status = null,
        TaskPriority? priority = null,
        int? order = null,
        Guid? parentId = null,
        Guid? teamId = null,
        FlexibleDateRange? plannedDateRange = null,
        FlexibleDateRange? actualDateRange = null,
        LocalDate? plannedDate = null,
        LocalDate? actualDate = null,
        decimal? estimatedEffortHours = null,
        decimal? actualEffortHours = null)
    {
        if (id.HasValue) { faker.RuleFor(x => x.Id, id.Value); }
        if (key.HasValue) { faker.RuleFor(x => x.Key, key.Value); }
        if (taskKey is not null) { faker.RuleFor(x => x.TaskKey, taskKey); }
        if (projectId.HasValue) { faker.RuleFor(x => x.ProjectId, projectId.Value); }
        if (!string.IsNullOrWhiteSpace(name)) { faker.RuleFor(x => x.Name, name); }
        if (!string.IsNullOrWhiteSpace(description)) { faker.RuleFor(x => x.Description, description); }
        if (type.HasValue) { faker.RuleFor(x => x.Type, type.Value); }
        if (status.HasValue) { faker.RuleFor(x => x.Status, status.Value); }
        if (priority.HasValue) { faker.RuleFor(x => x.Priority, priority); }
        if (order.HasValue) { faker.RuleFor(x => x.Order, order.Value); }
        if (parentId.HasValue) { faker.RuleFor(x => x.ParentId, parentId); }
        if (teamId.HasValue) { faker.RuleFor(x => x.TeamId, teamId); }
        if (plannedDateRange is not null) { faker.RuleFor(x => x.PlannedDateRange, plannedDateRange); }
        if (actualDateRange is not null) { faker.RuleFor(x => x.ActualDateRange, actualDateRange); }
        if (plannedDate.HasValue) { faker.RuleFor(x => x.PlannedDate, plannedDate); }
        if (actualDate.HasValue) { faker.RuleFor(x => x.ActualDate, actualDate); }
        if (estimatedEffortHours.HasValue) { faker.RuleFor(x => x.EstimatedEffortHours, estimatedEffortHours); }
        if (actualEffortHours.HasValue) { faker.RuleFor(x => x.ActualEffortHours, actualEffortHours); }

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

        return faker.WithData(
            projectId: projectId,
            taskKey: new ProjectTaskKey(projectKey, new Random().Next(1, 999)),
            status: TaskStatus.NotStarted,
            plannedDateRange: new FlexibleDateRange(startDate, endDate)
        ).Generate();
    }

    /// <summary>
    /// Generates a task in InProgress status with actual start date.
    /// </summary>
    public static ProjectTask AsInProgress(this ProjectTaskFaker faker, TestingDateTimeProvider dateTimeProvider, Guid projectId, ProjectKey projectKey)
    {
        var now = dateTimeProvider.Today;
        var startDate = now.PlusDays(-5);
        var endDate = now.PlusDays(10);

        return faker.WithData(
            projectId: projectId,
            taskKey: new ProjectTaskKey(projectKey, new Random().Next(1, 999)),
            status: TaskStatus.InProgress,
            plannedDateRange: new FlexibleDateRange(startDate, endDate),
            actualDateRange: new FlexibleDateRange(startDate, null)
        ).Generate();
    }

    /// <summary>
    /// Generates a task in Completed status with actual dates.
    /// </summary>
    public static ProjectTask AsCompleted(this ProjectTaskFaker faker, TestingDateTimeProvider dateTimeProvider, Guid projectId, ProjectKey projectKey)
    {
        var now = dateTimeProvider.Today;
        var startDate = now.PlusDays(-15);
        var endDate = now.PlusDays(-5);

        return faker.WithData(
            projectId: projectId,
            taskKey: new ProjectTaskKey(projectKey, new Random().Next(1, 999)),
            status: TaskStatus.Completed,
            plannedDateRange: new FlexibleDateRange(startDate, endDate),
            actualDateRange: new FlexibleDateRange(startDate, endDate)
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
            taskKey: new ProjectTaskKey(projectKey, new Random().Next(1, 999)),
            type: ProjectTaskType.Milestone,
            plannedDate: milestoneDate
        ).Generate();
    }
}
