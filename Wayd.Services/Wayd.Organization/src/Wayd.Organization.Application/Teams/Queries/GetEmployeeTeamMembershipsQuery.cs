using Mapster;

namespace Wayd.Organization.Application.Teams.Queries;

public sealed record GetEmployeeTeamMembershipsQuery(Guid EmployeeId) : IQuery<IReadOnlyList<TeamMemberDto>>;

internal sealed class GetEmployeeTeamMembershipsQueryHandler(IOrganizationDbContext organizationDbContext) : IQueryHandler<GetEmployeeTeamMembershipsQuery, IReadOnlyList<TeamMemberDto>>
{
    public async Task<IReadOnlyList<TeamMemberDto>> Handle(GetEmployeeTeamMembershipsQuery request, CancellationToken cancellationToken)
    {
        var memberships = await organizationDbContext.TeamMembers
            .Where(m => m.EmployeeId == request.EmployeeId)
            .ProjectToType<TeamMemberDto>()
            .ToListAsync(cancellationToken);

        return [.. memberships.OrderBy(m => m.Team.Name).ThenBy(m => m.Role.Name)];
    }
}
