using System.Linq.Expressions;
using Moda.Common.Application.Models;
using Moda.ProjectPortfolioManagement.Application.Projects.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Models.StrategicInitiatives;

namespace Moda.ProjectPortfolioManagement.Application.StrategicInitiatives.Queries;
public sealed record GetStrategicInitiativeProjectsQuery : IQuery<List<ProjectListDto>?>
{
    public GetStrategicInitiativeProjectsQuery(IdOrKey strategicInitiativeIdOrKey)
    {
        StrategicInitiativeIdOrKeyFilter = strategicInitiativeIdOrKey.CreateFilter<StrategicInitiative>();
    }

    public Expression<Func<StrategicInitiative, bool>> StrategicInitiativeIdOrKeyFilter { get; }
}

internal sealed class GetStrategicInitiativeProjectsQueryHandler(IProjectPortfolioManagementDbContext projectPortfolioManagementDbContext)
    : IQueryHandler<GetStrategicInitiativeProjectsQuery, List<ProjectListDto>?>
{
    private readonly IProjectPortfolioManagementDbContext _ppmDbContext = projectPortfolioManagementDbContext;

    public async Task<List<ProjectListDto>?> Handle(GetStrategicInitiativeProjectsQuery request, CancellationToken cancellationToken)
    {
        var query = _ppmDbContext.StrategicInitiatives
            .Where(request.StrategicInitiativeIdOrKeyFilter);

        if (!await query.AnyAsync(cancellationToken))
        {
            return null;
        }

        return await query
            .SelectMany(i => i.StrategicInitiativeProjects.Select(ip => ip.Project))
            .ProjectToType<ProjectListDto>()
            .ToListAsync(cancellationToken);
    }
}
