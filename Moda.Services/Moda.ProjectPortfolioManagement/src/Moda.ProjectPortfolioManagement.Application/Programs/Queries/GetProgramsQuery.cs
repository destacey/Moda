using Moda.Common.Application.Models;
using Moda.ProjectPortfolioManagement.Application.Programs.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Enums;

namespace Moda.ProjectPortfolioManagement.Application.Programs.Queries;

public sealed record GetProgramsQuery(ProgramStatus? StatusFilter = null, IdOrKey? PortfolioIdOrKey = null) : IQuery<List<ProgramListDto>>;

internal sealed class GetProgramsQueryHandler(IProjectPortfolioManagementDbContext ppmDbContext) 
    : IQueryHandler<GetProgramsQuery, List<ProgramListDto>>
{
    private readonly IProjectPortfolioManagementDbContext _ppmDbContext = ppmDbContext;

    public async Task<List<ProgramListDto>> Handle(GetProgramsQuery request, CancellationToken cancellationToken)
    {
        var query = _ppmDbContext.Programs.AsQueryable();

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

        return await query.ProjectToType<ProgramListDto>().ToListAsync(cancellationToken);
    }
}