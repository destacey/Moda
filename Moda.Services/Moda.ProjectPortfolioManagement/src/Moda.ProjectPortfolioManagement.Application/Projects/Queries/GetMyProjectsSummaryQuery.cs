using Moda.ProjectPortfolioManagement.Application.Projects.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Application.Projects.Queries;

public sealed record GetMyProjectsSummaryQuery(ProjectStatus[]? StatusFilter = null) : IQuery<MyProjectsSummaryDto?>;

internal sealed class GetMyProjectsSummaryQueryHandler(IProjectPortfolioManagementDbContext ppmDbContext, ICurrentUser currentUser)
    : IQueryHandler<GetMyProjectsSummaryQuery, MyProjectsSummaryDto?>
{
    private readonly IProjectPortfolioManagementDbContext _ppmDbContext = ppmDbContext;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<MyProjectsSummaryDto?> Handle(GetMyProjectsSummaryQuery request, CancellationToken cancellationToken)
    {
        var employeeId = _currentUser.GetEmployeeId();
        if (!employeeId.HasValue)
        {
            return new MyProjectsSummaryDto();
        }

        var eid = employeeId.Value;

        IQueryable<Project> query = _ppmDbContext.Projects;

        if (request.StatusFilter is { Length: > 0 })
        {
            query = query.Where(p => request.StatusFilter.Contains(p.Status));
        }

        var sponsorCount = await query
            .CountAsync(p => p.Roles.Any(r => r.EmployeeId == eid && r.Role == ProjectRole.Sponsor), cancellationToken);

        var ownerCount = await query
            .CountAsync(p => p.Roles.Any(r => r.EmployeeId == eid && r.Role == ProjectRole.Owner), cancellationToken);

        var managerCount = await query
            .CountAsync(p => p.Roles.Any(r => r.EmployeeId == eid && r.Role == ProjectRole.Manager), cancellationToken);

        var memberCount = await query
            .CountAsync(p => p.Roles.Any(r => r.EmployeeId == eid && r.Role == ProjectRole.Member), cancellationToken);

        var assigneeCount = await query
            .CountAsync(p => p.Tasks.Any(t => t.Roles.Any(r => r.EmployeeId == eid && r.Role == TaskRole.Assignee)), cancellationToken);

        // Total is distinct projects (a user with multiple roles on the same project should count once)
        var totalCount = await query
            .CountAsync(p =>
                p.Roles.Any(r => r.EmployeeId == eid)
                || p.Tasks.Any(t => t.Roles.Any(r => r.EmployeeId == eid && r.Role == TaskRole.Assignee)), cancellationToken);

        return new MyProjectsSummaryDto
        {
            TotalCount = totalCount,
            SponsorCount = sponsorCount,
            OwnerCount = ownerCount,
            ManagerCount = managerCount,
            MemberCount = memberCount,
            AssigneeCount = assigneeCount,
        };
    }
}
