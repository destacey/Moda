using System.Linq.Expressions;
using Moda.Common.Application.Models;
using Moda.ProjectPortfolioManagement.Application.StrategicInitiatives.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Application.StrategicInitiatives.Queries;

public sealed record GetStrategicInitiativeQuery : IQuery<StrategicInitiativeDetailsDto?>
{
    public GetStrategicInitiativeQuery(IdOrKey idOrKey)
    {
        IdOrKeyFilter = idOrKey.CreateFilter<StrategicInitiative>();
    }

    public Expression<Func<StrategicInitiative, bool>> IdOrKeyFilter { get; }
}

internal sealed class GetStrategicInitiativeQueryHandler(IProjectPortfolioManagementDbContext projectPortfolioManagementDbContext)
    : IQueryHandler<GetStrategicInitiativeQuery, StrategicInitiativeDetailsDto?>
{
    private readonly IProjectPortfolioManagementDbContext _projectPortfolioManagementDbContext = projectPortfolioManagementDbContext;
    public async Task<StrategicInitiativeDetailsDto?> Handle(GetStrategicInitiativeQuery request, CancellationToken cancellationToken)
    {
        return await _projectPortfolioManagementDbContext.StrategicInitiatives
            .Where(request.IdOrKeyFilter)
            .ProjectToType<StrategicInitiativeDetailsDto>()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
