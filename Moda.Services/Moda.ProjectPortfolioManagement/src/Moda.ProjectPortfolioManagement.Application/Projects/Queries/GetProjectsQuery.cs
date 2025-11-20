using Moda.Common.Application.Models;
using Moda.ProjectPortfolioManagement.Application.Projects.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Enums;

namespace Moda.ProjectPortfolioManagement.Application.Projects.Queries;

public sealed record GetProjectsQuery(ProjectStatus? StatusFilter = null, IdOrKey? PortfolioIdOrKey = null, IdOrKey? ProgramIdOrKey = null) : IQuery<List<ProjectListDto>>;

internal sealed class GetProjectsQueryHandler(IProjectPortfolioManagementDbContext ppmDbContext) 
    : IQueryHandler<GetProjectsQuery, List<ProjectListDto>>
{
    private readonly IProjectPortfolioManagementDbContext _ppmDbContext = ppmDbContext;

    public async Task<List<ProjectListDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var query = _ppmDbContext.Projects.AsQueryable();

        if (request.StatusFilter.HasValue)
        {
            query = query.Where(pp => pp.Status == request.StatusFilter.Value);
        }

        if (request.PortfolioIdOrKey is not null)
        {
            // TODO: make this reusable
            Guid? portfolioId = request.PortfolioIdOrKey.IsId 
                ? request.PortfolioIdOrKey.AsId 
                : await _ppmDbContext.Portfolios
                    .Where(p => p.Key == request.PortfolioIdOrKey.AsKey)
                    .Select(p => (Guid?)p.Id)
                    .FirstOrDefaultAsync(cancellationToken);

            if (portfolioId is null)
            {
                return [];
            }

            query = query.Where(pp => pp.PortfolioId == portfolioId);
        }

        if (request.ProgramIdOrKey is not null)
        {
            Guid? programId = request.ProgramIdOrKey.IsId
                ? request.ProgramIdOrKey.AsId
                : await _ppmDbContext.Programs
                    .Where(p => p.Key == request.ProgramIdOrKey.AsKey)
                    .Select(p => (Guid?)p.Id)
                    .FirstOrDefaultAsync(cancellationToken);

            if (programId is null)
            {
                return [];
            }

            query = query.Where(pp => pp.ProgramId == programId);
        }

        return await query.ProjectToType<ProjectListDto>().ToListAsync(cancellationToken);
    }
}
