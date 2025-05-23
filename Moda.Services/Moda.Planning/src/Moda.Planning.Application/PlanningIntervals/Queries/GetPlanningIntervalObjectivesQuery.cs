﻿using System.Linq.Expressions;
using MediatR;
using Moda.Common.Application.Dtos;
using Moda.Common.Application.Models;
using Moda.Goals.Application.Objectives.Queries;
using Moda.Planning.Application.PlanningIntervals.Dtos;

namespace Moda.Planning.Application.PlanningIntervals.Queries;

public sealed record GetPlanningIntervalObjectivesQuery : IQuery<IReadOnlyList<PlanningIntervalObjectiveListDto>>
{
    public GetPlanningIntervalObjectivesQuery(IdOrKey idOrKey, Guid? teamId)
    {
        PlanningIntervalIdOrKeyFilter = idOrKey.CreateFilter<PlanningInterval>();
        TeamId = teamId;
    }

    public Expression<Func<PlanningInterval, bool>> PlanningIntervalIdOrKeyFilter { get; }
    public Guid? TeamId { get; set; }
}

internal sealed class GetPlanningIntervalObjectivesQueryHandler(IPlanningDbContext planningDbContext, ILogger<GetPlanningIntervalObjectivesQueryHandler> logger, ISender sender, IDateTimeProvider dateTimeProvider) : IQueryHandler<GetPlanningIntervalObjectivesQuery, IReadOnlyList<PlanningIntervalObjectiveListDto>>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;
    private readonly ILogger<GetPlanningIntervalObjectivesQueryHandler> _logger = logger;
    private readonly ISender _sender = sender;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    public async Task<IReadOnlyList<PlanningIntervalObjectiveListDto>> Handle(GetPlanningIntervalObjectivesQuery request, CancellationToken cancellationToken)
    {
        var query = _planningDbContext.PlanningIntervals.AsQueryable();

        if (request.TeamId.HasValue)
        {
            var piTeamExists = await _planningDbContext.PlanningIntervals
                .Where(request.PlanningIntervalIdOrKeyFilter)
                .AnyAsync(p => p.Teams.Any(t => t.TeamId == request.TeamId.Value), cancellationToken);
            if (!piTeamExists)
            {
                ThrowAndLogException(request, $"Planning interval does not have team {request.TeamId}.");
            }

            query = query
                .Include(p => p.Objectives.Where(o => o.TeamId == request.TeamId.Value))
                    .ThenInclude(o => o.Team)
                .Include(p => p.Objectives.Where(o => o.TeamId == request.TeamId.Value))
                    .ThenInclude(o => o.HealthCheck);
        }
        else
        {
            query = query
                .Include(p => p.Objectives)
                    .ThenInclude(o => o.Team)
                .Include(p => p.Objectives)
                    .ThenInclude(o => o.HealthCheck);
        }

        var planningInterval = await query
            .Where(request.PlanningIntervalIdOrKeyFilter)
            .AsNoTrackingWithIdentityResolution()
            .FirstOrDefaultAsync(cancellationToken);
        if (planningInterval is null || planningInterval.Objectives.Count == 0)
            return [];

        // call the objective query handler
        var teamIds = request.TeamId.HasValue ? new Guid[] { request.TeamId.Value } : null;
        var objectives = await _sender.Send(new GetObjectivesForPlanningIntervalsQuery([planningInterval.Id], teamIds), cancellationToken);
        if (!objectives.Any() || planningInterval.Objectives.Count != objectives.Count)
            ThrowAndLogException(request, $"Error mapping objectives for planning interval {planningInterval.Id}.");

        // map the list of objectives
        var piNavigation = NavigationDto.Create(planningInterval.Id, planningInterval.Key, planningInterval.Name);
        List<PlanningIntervalObjectiveListDto> piObjectives = new(objectives.Count);
        foreach (var piObjective in planningInterval.Objectives)
        {
            piObjectives.Add(PlanningIntervalObjectiveListDto.Create(piObjective, objectives.Single(o => o.Id == piObjective.ObjectiveId), piNavigation, _dateTimeProvider.Now));
        }

        return piObjectives;
    }

    private void ThrowAndLogException(GetPlanningIntervalObjectivesQuery request, string message)
    {
        var requestName = request.GetType().Name;
        var exception = new InternalServerException(message);

        _logger.LogError(exception, "Moda Request: Exception for Request {Name} {@Request}. {Message}", requestName, request, message);
        throw exception;
    }
}
