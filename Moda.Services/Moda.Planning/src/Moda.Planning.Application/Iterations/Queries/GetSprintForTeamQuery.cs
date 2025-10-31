using Moda.Common.Domain.Enums.Planning;
using Moda.Planning.Application.Iterations.Dtos;

namespace Moda.Planning.Application.Iterations.Queries;
public sealed record GetSprintForTeamQuery(Guid TeamId) : IQuery<List<SprintListDto>>;

internal sealed class GetSprintForTeamQueryHandler(IPlanningDbContext planningDbContext)
    : IQueryHandler<GetSprintForTeamQuery, List<SprintListDto>>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;

    public async Task<List<SprintListDto>> Handle(GetSprintForTeamQuery request, CancellationToken cancellationToken)
    {
        var sprints = await _planningDbContext.Iterations
            .Where(i => i.TeamId == request.TeamId && i.Type == IterationType.Sprint)
            .ProjectToType<SprintListDto>()
            .ToListAsync(cancellationToken);

        return sprints;
    }
}
