using Mapster;
using NodaTime;

namespace Moda.Organization.Application.Teams.Queries;

/// <summary>
/// Gets the operating models for multiple teams. When AsOfDate is null, returns current models.
/// When AsOfDate is specified, returns the models that were effective on that date.
/// Teams without an operating model are not included in the result dictionary.
/// </summary>
public sealed record GetTeamOperatingModelsQuery(IEnumerable<Guid> TeamIds, LocalDate? AsOfDate = null)
    : IQuery<Dictionary<Guid, TeamOperatingModelDto>>;

internal sealed class GetTeamOperatingModelsQueryHandler(IOrganizationDbContext organizationDbContext)
    : IQueryHandler<GetTeamOperatingModelsQuery, Dictionary<Guid, TeamOperatingModelDto>>
{
    private readonly IOrganizationDbContext _organizationDbContext = organizationDbContext;

    public async Task<Dictionary<Guid, TeamOperatingModelDto>> Handle(GetTeamOperatingModelsQuery request, CancellationToken cancellationToken)
    {
        var teamIdList = request.TeamIds.ToList();

        if (teamIdList.Count == 0)
        {
            return [];
        }

        var query = _organizationDbContext.TeamOperatingModels
            .Where(m => teamIdList.Contains(m.TeamId));

        if (request.AsOfDate is null)
        {
            // Return current models (End is null)
            query = query.Where(m => m.DateRange.End == null);
        }
        else
        {
            // Return models effective on the specified date
            var asOfDate = request.AsOfDate.Value;
            query = query.Where(m =>
                m.DateRange.Start <= asOfDate &&
                (m.DateRange.End == null || m.DateRange.End >= asOfDate));
        }

        var models = await query
            .ProjectToType<TeamOperatingModelDto>()
            .ToListAsync(cancellationToken);

        return models.ToDictionary(m => m.TeamId);
    }
}
