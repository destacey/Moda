using System.Linq.Expressions;
using Wayd.Common.Application.Models;
using Wayd.ProjectPortfolioManagement.Application.Projects.Dtos;
using Wayd.ProjectPortfolioManagement.Application.Projects.Models;
using Wayd.ProjectPortfolioManagement.Domain.Models;

namespace Wayd.ProjectPortfolioManagement.Application.Projects.Queries;

public sealed record GetProjectQuery : IQuery<ProjectDetailsDto?>
{
    public GetProjectQuery(ProjectIdOrKey idOrKey)
    {
        IdOrKeyFilter = idOrKey.CreateFilter<Project>();
    }

    public Expression<Func<Project, bool>> IdOrKeyFilter { get; }
}

internal sealed class GetProjectQueryHandler(
    IProjectPortfolioManagementDbContext ppmDbContext,
    IDateTimeProvider dateTimeProvider,
    ICurrentUser currentUser)
    : IQueryHandler<GetProjectQuery, ProjectDetailsDto?>
{
    private readonly IProjectPortfolioManagementDbContext _ppmDbContext = ppmDbContext;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly ICurrentUser _currentUser = currentUser;

    public async Task<ProjectDetailsDto?> Handle(GetProjectQuery request, CancellationToken cancellationToken)
    {
        var project = await _ppmDbContext.Projects
            .AsSplitQuery()
            .Include(p => p.HealthChecks)
            .Include(p => p.Portfolio).ThenInclude(p => p!.Roles)
            .Include(p => p.Program).ThenInclude(p => p!.Roles)
            .Where(request.IdOrKeyFilter)
            .FirstOrDefaultAsync(cancellationToken);

        if (project is null)
            return null;

        var dto = project.Adapt<ProjectDetailsDto>();

        var now = _dateTimeProvider.Now;
        dto.HealthCheck = ProjectHealthCheckDto.FromCurrent(project.HealthChecks, now);

        var employeeId = _currentUser.GetEmployeeId();
        if (employeeId.HasValue)
        {
            dto.CanManageHealthChecks = project.CanManageHealthChecks(
                employeeId.Value,
                project.Portfolio!.Roles,
                project.Program?.Roles);
        }

        return dto;
    }
}
