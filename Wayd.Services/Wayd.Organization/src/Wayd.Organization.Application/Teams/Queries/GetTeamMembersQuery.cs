using Mapster;

namespace Wayd.Organization.Application.Teams.Queries;

public sealed record GetTeamMembersQuery(Guid TeamId) : IQuery<IReadOnlyList<TeamMemberDto>>;

internal sealed class GetTeamMembersQueryHandler(IOrganizationDbContext organizationDbContext) : IQueryHandler<GetTeamMembersQuery, IReadOnlyList<TeamMemberDto>>
{
    public async Task<IReadOnlyList<TeamMemberDto>> Handle(GetTeamMembersQuery request, CancellationToken cancellationToken)
    {
        var members = await organizationDbContext.BaseTeams
            .Where(t => t.Id == request.TeamId)
            .SelectMany(t => t.Members)
            .ProjectToType<TeamMemberDto>()
            .ToListAsync(cancellationToken);

        return [.. members.OrderBy(m => m.Employee.Name).ThenBy(m => m.Role.Name)];
    }
}
