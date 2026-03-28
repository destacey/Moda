using Moda.ProjectPortfolioManagement.Application.Projects.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Enums;

namespace Moda.ProjectPortfolioManagement.Application.Projects.Queries;

/// <summary>
/// Returns plan summary metrics for multiple projects in a single query.
/// Applies the same per-project leadership vs. assignee visibility rules
/// as <see cref="GetMyProjectsTaskMetricsQuery"/>: when the user holds a
/// selected leadership role on a project, all tasks are visible; otherwise,
/// only tasks assigned to the user are counted.
/// </summary>
public sealed record GetProjectsPlanSummariesQuery(
    Guid[] ProjectIds,
    ProjectMemberRole[]? RoleFilter = null) : IQuery<Dictionary<Guid, ProjectPlanSummaryDto>>;

internal sealed class GetProjectsPlanSummariesQueryHandler(
    IProjectPortfolioManagementDbContext ppmDbContext,
    ICurrentUser currentUser,
    IDateTimeProvider dateTimeProvider)
    : IQueryHandler<GetProjectsPlanSummariesQuery, Dictionary<Guid, ProjectPlanSummaryDto>>
{
    private readonly IProjectPortfolioManagementDbContext _ppmDbContext = ppmDbContext;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    public async Task<Dictionary<Guid, ProjectPlanSummaryDto>> Handle(
        GetProjectsPlanSummariesQuery request,
        CancellationToken cancellationToken)
    {
        if (request.ProjectIds.Length == 0)
        {
            return [];
        }

        var employeeId = _currentUser.GetEmployeeId();
        if (!employeeId.HasValue)
        {
            return [];
        }

        var eid = employeeId.Value;
        var today = _dateTimeProvider.Today;

        var daysUntilSaturday = ((int)IsoDayOfWeek.Saturday - (int)today.DayOfWeek + 7) % 7;
        var endOfThisWeek = today.PlusDays(daysUntilSaturday);
        var endOfNextWeek = endOfThisWeek.PlusDays(7);

        var openStatuses = new[] { Domain.Enums.TaskStatus.NotStarted, Domain.Enums.TaskStatus.InProgress };
        var allLeadershipRoles = new[] { ProjectRole.Sponsor, ProjectRole.Owner, ProjectRole.Manager };

        var activeLeadershipRoles = request.RoleFilter is { Length: > 0 }
            ? request.RoleFilter
                .Where(r => r != ProjectMemberRole.Assignee && r != ProjectMemberRole.Member)
                .Select(r => (ProjectRole)(int)r)
                .ToArray()
            : allLeadershipRoles;

        // All leaf tasks across requested projects
        var leafTasks = _ppmDbContext.ProjectTasks
            .Where(t => request.ProjectIds.Contains(t.ProjectId))
            .Where(t => !_ppmDbContext.ProjectTasks.Any(child => child.ParentId == t.Id));

        // Open leaf tasks with a planned end date
        var openLeafTasks = leafTasks
            .Where(t => openStatuses.Contains(t.Status))
            .Where(t => t.PlannedDateRange != null && t.PlannedDateRange.End != null);

        IQueryable<Domain.Models.ProjectTask> relevantTasks;

        if (activeLeadershipRoles.Length > 0)
        {
            var leadershipTasks = openLeafTasks
                .Where(t => t.Project.Roles.Any(r => r.EmployeeId == eid && activeLeadershipRoles.Contains(r.Role)));

            var assigneeTasks = openLeafTasks
                .Where(t => !t.Project.Roles.Any(r => r.EmployeeId == eid && activeLeadershipRoles.Contains(r.Role)))
                .Where(t => t.Roles.Any(r => r.EmployeeId == eid && r.Role == TaskRole.Assignee));

            relevantTasks = leadershipTasks.Concat(assigneeTasks);
        }
        else
        {
            relevantTasks = openLeafTasks
                .Where(t => t.Roles.Any(r => r.EmployeeId == eid && r.Role == TaskRole.Assignee));
        }

        // Project the minimal data needed, then group in memory.
        // EF Core cannot translate GroupBy with conditional counts on owned type properties.
        var taskData = await relevantTasks
            .Select(t => new { t.ProjectId, EndDate = t.PlannedDateRange!.End })
            .ToListAsync(cancellationToken);

        var totalLeafCounts = await leafTasks
            .GroupBy(t => t.ProjectId)
            .Select(g => new { ProjectId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var totalLeafMap = totalLeafCounts.ToDictionary(x => x.ProjectId, x => x.Count);

        return taskData
            .GroupBy(t => t.ProjectId)
            .ToDictionary(
                g => g.Key,
                g => new ProjectPlanSummaryDto
                {
                    Overdue = g.Count(t => t.EndDate < today),
                    DueThisWeek = g.Count(t => t.EndDate >= today && t.EndDate <= endOfThisWeek),
                    Upcoming = g.Count(t => t.EndDate > endOfThisWeek && t.EndDate <= endOfNextWeek),
                    TotalLeafTasks = totalLeafMap.GetValueOrDefault(g.Key),
                });
    }
}
