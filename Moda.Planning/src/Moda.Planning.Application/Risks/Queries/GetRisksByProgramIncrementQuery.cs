using MediatR;
using Moda.Common.Extensions;
using Moda.Planning.Application.ProgramIncrements.Queries;
using Moda.Planning.Application.Risks.Dtos;
using Moda.Planning.Domain.Enums;

namespace Moda.Planning.Application.Risks.Queries;

public sealed record GetRisksByProgramIncrementQuery(Guid ProgramIncrementId, bool IncludeClosed) : IQuery<IReadOnlyList<RiskListDto>>;

internal sealed class GetRisksByProgramIncrementQueryHandler : IQueryHandler<GetRisksByProgramIncrementQuery, IReadOnlyList<RiskListDto>>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly ISender _sender;

    public GetRisksByProgramIncrementQueryHandler(IPlanningDbContext planningDbContext, ISender sender)
    {
        _planningDbContext = planningDbContext;
        _sender = sender;
    }

    public async Task<IReadOnlyList<RiskListDto>> Handle(GetRisksByProgramIncrementQuery request, CancellationToken cancellationToken)
    {
        var teamIds = await _sender.Send(new GetProgramIncrementTeamsQuery(request.ProgramIncrementId), cancellationToken);
        if (!teamIds.Any())
            return new List<RiskListDto>();

        var piDates = await _planningDbContext.ProgramIncrements
            .Where(p => p.Id == request.ProgramIncrementId)
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
            .Where(r => r.TeamId.HasValue && teamIds.ToList().Contains(r.TeamId.Value))
            .Where(r => r.ReportedOn <= piDates.end && (!r.ClosedDate.HasValue || piDates.start <= r.ClosedDate.Value))
            .AsQueryable();

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
