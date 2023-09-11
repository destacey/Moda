using Moda.Goals.Application.Objectives.Dtos;
using Moda.Goals.Application.Persistence;

namespace Moda.Goals.Application.Objectives.Queries;
public sealed record GetObjectivesForProgramIncrementsQuery : IQuery<IReadOnlyList<ObjectiveListDto>>
{
    public GetObjectivesForProgramIncrementsQuery(Guid[] programIncrementIds, Guid[]? teamIds)
    {
        ProgramIncrementIds = programIncrementIds;
        TeamIds = teamIds;
    }

    public Guid[] ProgramIncrementIds { get; }
    public Guid[]? TeamIds { get; }
}

internal sealed class GetObjectivesForProgramIncrementsQueryHandler : IQueryHandler<GetObjectivesForProgramIncrementsQuery, IReadOnlyList<ObjectiveListDto>>
{
    private readonly IGoalsDbContext _goalsDbContext;

    public GetObjectivesForProgramIncrementsQueryHandler(IGoalsDbContext goalsDbContext)
    {
        _goalsDbContext = goalsDbContext;
    }

    public async Task<IReadOnlyList<ObjectiveListDto>> Handle(GetObjectivesForProgramIncrementsQuery request, CancellationToken cancellationToken)
    {
        var query = _goalsDbContext.Objectives
            .Where(o => o.PlanId.HasValue && request.ProgramIncrementIds.Contains(o.PlanId.Value))
            .AsQueryable();

        if (request.TeamIds?.Any() ?? false)
        {
            query = query.Where(o => o.OwnerId.HasValue && request.TeamIds.Contains(o.OwnerId.Value));
        }

        return await query
            .ProjectToType<ObjectiveListDto>()
            .ToListAsync(cancellationToken);
    }
}
