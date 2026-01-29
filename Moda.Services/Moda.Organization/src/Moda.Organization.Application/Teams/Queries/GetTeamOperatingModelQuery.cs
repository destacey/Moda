using Mapster;
using NodaTime;

namespace Moda.Organization.Application.Teams.Queries;

/// <summary>
/// Gets the operating model for a team. When AsOfDate is null, returns the current model.
/// When AsOfDate is specified, returns the model that was effective on that date.
/// Returns null if no operating model is defined for the team.
/// </summary>
public sealed record GetTeamOperatingModelQuery(Guid TeamId, LocalDate? AsOfDate = null)
    : IQuery<TeamOperatingModelDto?>;

internal sealed class GetTeamOperatingModelQueryHandler(IOrganizationDbContext organizationDbContext)
    : IQueryHandler<GetTeamOperatingModelQuery, TeamOperatingModelDto?>
{
    private readonly IOrganizationDbContext _organizationDbContext = organizationDbContext;

    public async Task<TeamOperatingModelDto?> Handle(GetTeamOperatingModelQuery request, CancellationToken cancellationToken)
    {
        var query = _organizationDbContext.TeamOperatingModels
            .Where(m => m.TeamId == request.TeamId);

        if (request.AsOfDate is null)
        {
            // Return current model (End is null)
            query = query.Where(m => m.DateRange.End == null);
        }
        else
        {
            // Return model effective on the specified date
            var asOfDate = request.AsOfDate.Value;
            query = query.Where(m =>
                m.DateRange.Start <= asOfDate &&
                (m.DateRange.End == null || m.DateRange.End >= asOfDate));
        }

        return await query
            .ProjectToType<TeamOperatingModelDto>()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
