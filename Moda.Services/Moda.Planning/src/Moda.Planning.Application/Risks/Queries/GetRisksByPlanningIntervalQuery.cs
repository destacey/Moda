using MediatR;
using Moda.Common.Extensions;
using Moda.Planning.Application.PlanningIntervals.Queries;
using Moda.Planning.Application.Risks.Dtos;
using Moda.Planning.Domain.Enums;

namespace Moda.Planning.Application.Risks.Queries;

public sealed record GetRisksByPlanningIntervalQuery(Guid PlanningIntervalId, bool IncludeClosed, Guid? TeamId) : IQuery<IReadOnlyList<RiskListDto>>;

internal sealed class GetRisksByPlanningIntervalQueryHandler : IQueryHandler<GetRisksByPlanningIntervalQuery, IReadOnlyList<RiskListDto>>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly ISender _sender;

    public GetRisksByPlanningIntervalQueryHandler(IPlanningDbContext planningDbContext, ISender sender)
    {
        _planningDbContext = planningDbContext;
        _sender = sender;
    }

    public async Task<IReadOnlyList<RiskListDto>> Handle(GetRisksByPlanningIntervalQuery request, CancellationToken cancellationToken)
    {
        var teamIds = await _sender.Send(new GetPlanningIntervalTeamsQuery(request.PlanningIntervalId), cancellationToken);
        if (!teamIds.Any())
            return new List<RiskListDto>();

        if (request.TeamId.HasValue && !teamIds.Contains(request.TeamId.Value))
            return new List<RiskListDto>();

        var piDates = await _planningDbContext.PlanningIntervals
            .Where(p => p.Id == request.PlanningIntervalId)
            .Select(p => new 
            { 
                start = p.DateRange.Start.ToInstant(), 
                end = p.DateRange.End.PlusDays(1).ToInstant() 
            })
            .SingleAsync(cancellationToken);

        // there is still a gap in these dates

        var query = _planningDbContext.Risks
            .Include(r => r.Team)
            .Include(r => r.Assignee)            
            .Where(r => r.ReportedOn <= piDates.end && (!r.ClosedDate.HasValue || piDates.start <= r.ClosedDate.Value))
            .AsQueryable();

        if (request.TeamId.HasValue)
        {
            query = query.Where(r => r.TeamId == request.TeamId.Value);
        }
        else
        {
            query = query.Where(r => r.TeamId.HasValue && teamIds.ToList().Contains(r.TeamId.Value));
        }

        if (!request.IncludeClosed)
        {
            query = query.Where(r => r.Status != RiskStatus.Closed);
        }

        var risks = await query
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return risks.Adapt<IReadOnlyList<RiskListDto>>();
    }
}
