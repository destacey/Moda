using Moda.ProjectPortfolioManagement.Application.Projects.Dtos;

namespace Moda.ProjectPortfolioManagement.Application.Projects.Queries;

public sealed record GetProjectPhaseQuery(Guid ProjectId, Guid PhaseId) : IQuery<ProjectPhaseDetailsDto?>;

internal sealed class GetProjectPhaseQueryHandler(IProjectPortfolioManagementDbContext ppmDbContext)
    : IQueryHandler<GetProjectPhaseQuery, ProjectPhaseDetailsDto?>
{
    private readonly IProjectPortfolioManagementDbContext _ppmDbContext = ppmDbContext;

    public async Task<ProjectPhaseDetailsDto?> Handle(GetProjectPhaseQuery request, CancellationToken cancellationToken)
    {
        return await _ppmDbContext.ProjectPhases
            .Where(p => p.ProjectId == request.ProjectId && p.Id == request.PhaseId)
            .ProjectToType<ProjectPhaseDetailsDto>()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
