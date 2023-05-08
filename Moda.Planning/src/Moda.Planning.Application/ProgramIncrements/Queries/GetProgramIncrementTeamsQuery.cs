namespace Moda.Planning.Application.ProgramIncrements.Queries;

public sealed record GetProgramIncrementTeamsQuery(Guid Id) : IQuery<IReadOnlyList<Guid>>;

internal sealed class GetProgramIncrementTeamsQueryHandler : IQueryHandler<GetProgramIncrementTeamsQuery, IReadOnlyList<Guid>>
{
    private readonly IPlanningDbContext _planningDbContext;

    public GetProgramIncrementTeamsQueryHandler(IPlanningDbContext planningDbContext)
    {
        _planningDbContext = planningDbContext;
    }

    public async Task<IReadOnlyList<Guid>> Handle(GetProgramIncrementTeamsQuery request, CancellationToken cancellationToken)
    {
        return await _planningDbContext.ProgramIncrements
            .Where(p => p.Id == request.Id)
            .SelectMany(p => p.ProgramIncrementTeams.Select(t => t.TeamId))
            .ToListAsync(cancellationToken);
    }
}
