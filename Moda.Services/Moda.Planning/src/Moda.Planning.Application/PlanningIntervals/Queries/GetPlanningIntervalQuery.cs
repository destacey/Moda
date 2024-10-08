﻿using System.Linq.Expressions;
using Moda.Common.Application.Models;
using Moda.Planning.Application.PlanningIntervals.Dtos;

namespace Moda.Planning.Application.PlanningIntervals.Queries;
public sealed record GetPlanningIntervalQuery : IQuery<PlanningIntervalDetailsDto?>
{
    public GetPlanningIntervalQuery(IdOrKey idOrKey)
    {
        IdOrKeyFilter = idOrKey.CreateFilter<PlanningInterval>();
    }

    public Expression<Func<PlanningInterval, bool>> IdOrKeyFilter { get; }
}

internal sealed class GetPlanningIntervalQueryHandler : IQueryHandler<GetPlanningIntervalQuery, PlanningIntervalDetailsDto?>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly ILogger<GetPlanningIntervalQueryHandler> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetPlanningIntervalQueryHandler(IPlanningDbContext planningDbContext, ILogger<GetPlanningIntervalQueryHandler> logger, IDateTimeProvider dateTimeProvider)
    {
        _planningDbContext = planningDbContext;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<PlanningIntervalDetailsDto?> Handle(GetPlanningIntervalQuery request, CancellationToken cancellationToken)
    {
        return await _planningDbContext.PlanningIntervals
            .Include(p => p.Objectives)
            .Where(request.IdOrKeyFilter)
            .Select(p => PlanningIntervalDetailsDto.Create(p, _dateTimeProvider))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
