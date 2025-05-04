using Mapster;
using Neo4jClient;

namespace Moda.Organization.Application.Teams.Queries;
public sealed record GetTeamsNeo4jQuery : IQuery<IReadOnlyList<TeamListDto>>
{
    public GetTeamsNeo4jQuery(bool includeInactive = false, IEnumerable<Guid>? ids = null)
    {
        IncludeInactive = includeInactive;
        TeamIds = ids?.ToList() ?? new List<Guid>();
    }

    public bool IncludeInactive { get; }
    public List<Guid> TeamIds { get; }
}

internal sealed class GetTeamsNeo4jQueryHandler : IQueryHandler<GetTeamsNeo4jQuery, IReadOnlyList<TeamListDto>>
{
    //private readonly IOrganizationDbContext _organizationDbContext;
    private readonly IGraphClient _neo4jClient;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetTeamsNeo4jQueryHandler(IGraphClient neo4jClient, IDateTimeProvider dateTimeProvider)
    {
        _neo4jClient = neo4jClient;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<IReadOnlyList<TeamListDto>> Handle(GetTeamsNeo4jQuery request, CancellationToken cancellationToken)
    {
        var today = _dateTimeProvider.Now.InUtc().Date;
        // var query = _organizationDbContext.Teams
        //     .Include(t => t.ParentMemberships.Where(m => m.DateRange.Start <= today && (!m.DateRange.End.HasValue || today <= m.DateRange.End)))
        //     .AsQueryable();
        var query = _neo4jClient.Cypher
            .Match("(t:Team)-[r:UNDER]->(:Team)")
            .Where("r.Start <= date($today) AND (r.End IS NULL OR date($today) <= r.End)")
            .WithParam("today", today);

        if (!request.IncludeInactive)
            query = query.Where("t.IsActive = $isActive")
            .WithParam("isActive", true);

        if (request.TeamIds.Any())
            query = query.AndWhere("t.Id IN $teamIds")
            .WithParam("teamIds", request.TeamIds.Select(g => g.ToString()).ToList());

        return (IReadOnlyList<TeamListDto>)await query
            .Return(team => team.As<TeamListDto>()).ResultsAsync;
    }
}
