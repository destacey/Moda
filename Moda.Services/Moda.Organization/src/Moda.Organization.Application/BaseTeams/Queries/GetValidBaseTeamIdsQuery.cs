namespace Moda.Organization.Application.BaseTeams.Queries;
public sealed record GetValidBaseTeamIdsQuery() : IQuery<Guid[]>;
internal sealed class GetValidBaseTeamIdsQueryHandler : IQueryHandler<GetValidBaseTeamIdsQuery, Guid[]>
{
    private readonly IOrganizationDbContext _organizationDbContext;

    public GetValidBaseTeamIdsQueryHandler(IOrganizationDbContext organizationDbContext)
    {
        _organizationDbContext = organizationDbContext;
    }

    public async Task<Guid[]> Handle(GetValidBaseTeamIdsQuery request, CancellationToken cancellationToken)
    {
        return await _organizationDbContext.BaseTeams
            .Select(t => t.Id)
            .ToArrayAsync(cancellationToken);
    }
}
