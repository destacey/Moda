using CSharpFunctionalExtensions;
using CsvHelper;
using Mapster;
using Moda.Common.Application.Interfaces;
using Moda.Common.Application.Models;
using Moda.Common.Extensions;
using Moda.Health.Queries;
using Moda.Organization.Application.Teams.Queries;
using Moda.Organization.Application.TeamsOfTeams.Queries;
using Moda.Planning.Application.PlanningIntervals.Commands;
using Moda.Planning.Application.PlanningIntervals.Dtos;
using Moda.Planning.Application.PlanningIntervals.Queries;
using Moda.Planning.Application.Risks.Dtos;
using Moda.Planning.Application.Risks.Queries;
using Moda.Web.Api.Dtos.Planning;
using Moda.Web.Api.Extensions;
using Moda.Web.Api.Models.Planning.PlanningIntervals;
using Moda.Work.Application.WorkItems.Dtos;
using Moda.Work.Application.WorkItems.Queries;

namespace Moda.Web.Api.Controllers.Planning;

[Route("api/planning/planning-intervals")]
[ApiVersionNeutral]
[ApiController]
public class PlanningIntervalsController : ControllerBase
{
    private readonly ILogger<PlanningIntervalsController> _logger;
    private readonly ISender _sender;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICsvService _csvService;

    public PlanningIntervalsController(ILogger<PlanningIntervalsController> logger, ISender sender, IDateTimeProvider dateTimeProvider, ICsvService csvService)
    {
        _logger = logger;
        _sender = sender;
        _dateTimeProvider = dateTimeProvider;
        _csvService = csvService;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervals)]
    [OpenApiOperation("Get a list of planning intervals.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PlanningIntervalListDto>>> GetList(CancellationToken cancellationToken)
    {
        var planningIntervals = await _sender.Send(new GetPlanningIntervalsQuery(), cancellationToken);
        return Ok(planningIntervals);
    }

    [HttpGet("{idOrKey}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervals)]
    [OpenApiOperation("Get planning interval details.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlanningIntervalDetailsDto>> GetPlanningInterval(string idOrKey, CancellationToken cancellationToken)
    {
        var planningInterval = await _sender.Send(new GetPlanningIntervalQuery(idOrKey), cancellationToken);

        return planningInterval is not null
            ? Ok(planningInterval)
            : NotFound();
    }

    [HttpGet("{idOrKey}/calendar")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervals)]
    [OpenApiOperation("Get the PI calendar.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PlanningIntervalCalendarDto>> GetCalendar(string idOrKey, CancellationToken cancellationToken)
    {
        var calendar = await _sender.Send(new GetPlanningIntervalCalendarQuery(idOrKey), cancellationToken);

        return calendar is not null
            ? Ok(calendar)
            : NotFound();
    }

    [HttpGet("{idOrKey}/predictability")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervals)]
    [OpenApiOperation("Get the PI predictability for all teams.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PlanningIntervalPredictabilityDto>> GetPredictability(string idOrKey, CancellationToken cancellationToken)
    {
        var predictability = await _sender.Send(new GetPlanningIntervalPredictabilityQuery(idOrKey), cancellationToken);

        return predictability is not null
            ? Ok(predictability)
            : NotFound();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.PlanningIntervals)]
    [OpenApiOperation("Create a planning interval.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.CreateReturn201IdAndKey))]
    public async Task<ActionResult> Create([FromBody] CreatePlanningIntervalRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreatePlanningIntervalCommand(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetPlanningInterval), new { idOrKey = result.Value.Id.ToString() }, result.Value)
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPut("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.PlanningIntervals)]
    [OpenApiOperation("Update a planning interval.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdatePlanningIntervalRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(HttpContext));

        var result = await _sender.Send(request.ToUpdatePlanningIntervalCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpGet("{idOrKey}/teams")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervals)]
    [OpenApiOperation("Get a list of planning interval teams.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PlanningIntervalTeamResponse>>> GetTeams(string idOrKey, CancellationToken cancellationToken)
    {
        List<PlanningIntervalTeamResponse> piTeams = [];
        var teamIds = await _sender.Send(new GetPlanningIntervalTeamsQuery(idOrKey), cancellationToken);

        if (teamIds.Any())
        {
            var teams = await _sender.Send(new GetTeamsQuery(true, teamIds), cancellationToken);
            var teamOfTeams = await _sender.Send(new GetTeamOfTeamsListQuery(true, teamIds), cancellationToken);

            piTeams.AddRange(teams.Adapt<List<PlanningIntervalTeamResponse>>());
            piTeams.AddRange(teamOfTeams.Adapt<List<PlanningIntervalTeamResponse>>());
        }

        return Ok(piTeams);
    }

    [HttpGet("{idOrKey}/teams/{teamId}/predictability")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervals)]
    [OpenApiOperation("Get the PI predictability for a team.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<double?>> GetTeamPredictability(string idOrKey, Guid teamId, CancellationToken cancellationToken)
    {
        var predictability = await _sender.Send(new GetTeamPlanningIntervalPredictabilityQuery(idOrKey, teamId), cancellationToken);

        return Ok(predictability);
    }

    [HttpPost("{id}/dates")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.PlanningIntervals)]
    [OpenApiOperation("Manage planning interval dates and iterations.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ManageDates(Guid id, [FromBody] ManagePlanningIntervalDatesRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(HttpContext));

        var result = await _sender.Send(request.ToManagePlanningIntervalDatesCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPost("{id}/teams")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.PlanningIntervals)]
    [OpenApiOperation("Manage planning interval teams.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ManageTeams(Guid id, [FromBody] ManagePlanningIntervalTeamsRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(HttpContext));

        var result = await _sender.Send(request.ToManagePlanningIntervalTeamsCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    #region Iterations

    [HttpGet("{idOrKey}/iterations")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervals)]
    [OpenApiOperation("Get a list of planning interval iterations.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<PlanningIntervalIterationListDto>>> GetIterations(string idOrKey, CancellationToken cancellationToken)
    {
        var iterations = await _sender.Send(new GetPlanningIntervalIterationsQuery(idOrKey), cancellationToken);

        return iterations is not null 
            ? Ok(iterations) 
            : NotFound();
    }

    [HttpGet("{idOrKey}/iterations/{iterationIdOrKey}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervals)]
    [OpenApiOperation("Get a specific planning interval iteration.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlanningIntervalIterationDetailsDto?>> GetIteration(string idOrKey, string iterationIdOrKey, CancellationToken cancellationToken)
    {
        var iteration = await _sender.Send(new GetPlanningIntervalIterationQuery(idOrKey, iterationIdOrKey), cancellationToken);

        return iteration is not null
            ? Ok(iteration)
            : NotFound();
    }

    [HttpGet("iteration-categories")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervals)]
    [OpenApiOperation("Get a list of iteration categories.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<PlanningIntervalIterationCategoryDto>>> GetIterationCategories(CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new GetPlanningIntervalIterationCategoriesQuery(), cancellationToken);
        return Ok(items.OrderBy(c => c.Order));
    }

    [HttpGet("{idOrKey}/iterations/sprints")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervals)]
    [OpenApiOperation("Get iteration sprint mappings for a Planning Interval.", "Retrieves all sprint-to-iteration mappings, with optional filtering by iteration.")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<PlanningIntervalIterationSprintsDto>>> GetIterationSprints(string idOrKey, [FromQuery] Guid? iterationId, CancellationToken cancellationToken)
    {
        var iterations = await _sender.Send(new GetPlanningIntervalIterationSprintsQuery(idOrKey, iterationId), cancellationToken);

        return iterations is not null 
            ? Ok(iterations) 
            : NotFound();
    }

    [HttpPost("{id}/teams/{teamId}/sprints")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.PlanningIntervals)]
    [OpenApiOperation("Map team sprints to Planning Interval iterations.", "This is a sync/replace operation that sets the complete desired state for the team's sprint mappings.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> MapTeamSprints(Guid id, Guid teamId, [FromBody] MapPlanningIntervalSprintsRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(nameof(id), nameof(request.Id), HttpContext));

        if (teamId != request.TeamId)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(nameof(teamId), nameof(request.TeamId), HttpContext));

        var result = await _sender.Send(request.ToMapPlanningIntervalSprintsCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpGet("{idOrKey}/iterations/{iterationIdOrKey}/metrics")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervals)]
    [OpenApiOperation("Get metrics for a PI iteration aggregated across all mapped sprints.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlanningIntervalIterationMetricsResponse>> GetIterationMetrics(string idOrKey, string iterationIdOrKey, CancellationToken cancellationToken)
    {
        // Get the iteration with its mapped sprints from Planning context
        var iterationSprints = await _sender.Send(
            new GetPlanningIntervalIterationSprintsQuery(idOrKey, null),
            cancellationToken);

        if (iterationSprints is null)
            return NotFound();

        // Find the specific iteration
        var iterationKey = new IdOrKey(iterationIdOrKey);
        var iteration = iterationSprints.FirstOrDefault(i =>
            iterationKey.IsId ? i.Id == iterationKey.AsId : i.Key == iterationKey.AsKey);

        if (iteration is null)
            return NotFound();

        // Get work item metrics from Work context
        var sprintIds = iteration.Sprints.Select(s => s.Id).ToList();
        var sprintMetrics = await _sender.Send(
            new GetSprintsWorkItemMetricsQuery(sprintIds),
            cancellationToken);

        // Build per-sprint metrics summaries
        var sprintMetricsSummaries = iteration.Sprints.Select(sprint =>
        {
            var metrics = sprintMetrics.FirstOrDefault(m => m.SprintId == sprint.Id);
            return new SprintMetricsSummary
            {
                SprintId = sprint.Id,
                SprintKey = sprint.Key,
                SprintName = sprint.Name,
                State = sprint.State,
                Start = sprint.Start,
                End = sprint.End,
                Team = new Common.Application.Dtos.NavigationDto { Id = sprint.Team.Id, Key = sprint.Team.Key, Name = sprint.Team.Name },
                TotalWorkItems = metrics?.TotalWorkItems ?? 0,
                TotalStoryPoints = metrics?.TotalStoryPoints ?? 0,
                CompletedWorkItems = metrics?.CompletedWorkItems ?? 0,
                CompletedStoryPoints = metrics?.CompletedStoryPoints ?? 0,
                InProgressWorkItems = metrics?.InProgressWorkItems ?? 0,
                InProgressStoryPoints = metrics?.InProgressStoryPoints ?? 0,
                NotStartedWorkItems = metrics?.NotStartedWorkItems ?? 0,
                NotStartedStoryPoints = metrics?.NotStartedStoryPoints ?? 0,
                MissingStoryPointsCount = metrics?.MissingStoryPointsCount ?? 0,
                AverageCycleTimeDays = metrics?.AverageCycleTimeDays
            };
        }).ToList();

        // Calculate aggregate cycle time
        var allCycleTimes = sprintMetricsSummaries
            .Where(s => s.AverageCycleTimeDays.HasValue)
            .Select(s => s.AverageCycleTimeDays!.Value)
            .ToList();
        var avgCycleTime = allCycleTimes.Count > 0 ? allCycleTimes.Average() : (double?)null;

        return Ok(new PlanningIntervalIterationMetricsResponse
        {
            IterationId = iteration.Id,
            IterationKey = iteration.Key,
            IterationName = iteration.Name,
            Start = iteration.Start,
            End = iteration.End,
            Category = iteration.Category,
            TeamCount = iteration.Sprints.Select(s => s.Team.Id).Distinct().Count(),
            SprintCount = iteration.Sprints.Count,
            TotalWorkItems = sprintMetricsSummaries.Sum(s => s.TotalWorkItems),
            TotalStoryPoints = sprintMetricsSummaries.Sum(s => s.TotalStoryPoints),
            CompletedWorkItems = sprintMetricsSummaries.Sum(s => s.CompletedWorkItems),
            CompletedStoryPoints = sprintMetricsSummaries.Sum(s => s.CompletedStoryPoints),
            InProgressWorkItems = sprintMetricsSummaries.Sum(s => s.InProgressWorkItems),
            InProgressStoryPoints = sprintMetricsSummaries.Sum(s => s.InProgressStoryPoints),
            NotStartedWorkItems = sprintMetricsSummaries.Sum(s => s.NotStartedWorkItems),
            NotStartedStoryPoints = sprintMetricsSummaries.Sum(s => s.NotStartedStoryPoints),
            MissingStoryPointsCount = sprintMetricsSummaries.Sum(s => s.MissingStoryPointsCount),
            AverageCycleTimeDays = avgCycleTime,
            SprintMetrics = sprintMetricsSummaries
        });
    }

    [HttpGet("{idOrKey}/iterations/{iterationIdOrKey}/backlog")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervals)]
    [OpenApiOperation("Get combined backlog for a PI iteration from all mapped sprints.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<SprintBacklogItemDto>>> GetIterationBacklog(string idOrKey, string iterationIdOrKey, CancellationToken cancellationToken)
    {
        // Get the iteration with its mapped sprints from Planning context
        var iterationSprints = await _sender.Send(
            new GetPlanningIntervalIterationSprintsQuery(idOrKey, null),
            cancellationToken);

        if (iterationSprints is null)
            return NotFound();

        // Find the specific iteration
        var iterationKey = new IdOrKey(iterationIdOrKey);
        var iteration = iterationSprints.FirstOrDefault(i =>
            iterationKey.IsId ? i.Id == iterationKey.AsId : i.Key == iterationKey.AsKey);

        if (iteration is null)
            return NotFound();

        // Get combined backlog from Work context
        var sprintIds = iteration.Sprints.Select(s => s.Id).ToList();
        var backlog = await _sender.Send(
            new GetSprintsBacklogQuery(sprintIds),
            cancellationToken);

        return Ok(backlog);
    }

    #endregion Iterations

    #region Objectives

    [HttpGet("{idOrKey}/objectives")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervalObjectives)]
    [OpenApiOperation("Get a list of planning interval teams.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PlanningIntervalObjectiveListDto>>> GetObjectives(string idOrKey, Guid? teamId, CancellationToken cancellationToken)
    {
        var objectives = await _sender.Send(new GetPlanningIntervalObjectivesQuery(idOrKey, teamId), cancellationToken);

        return Ok(objectives);
    }

    [HttpGet("{idOrKey}/objectives/{objectiveIdOrKey}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervalObjectives)]
    [OpenApiOperation("Get a planning interval objective.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlanningIntervalObjectiveDetailsDto>> GetObjective(string idOrKey, string objectiveIdOrKey, CancellationToken cancellationToken)
    {
        var objective = await _sender.Send(new GetPlanningIntervalObjectiveQuery(idOrKey, objectiveIdOrKey), cancellationToken);

        return objective is not null
            ? Ok(objective)
            : NotFound();
    }

    [HttpPost("{id}/objectives")]
    [MustHavePermission(ApplicationAction.Manage, ApplicationResource.PlanningIntervalObjectives)]
    [OpenApiOperation("Create a planning interval objective.", "")]
    [ProducesResponseType(typeof(ObjectIdAndKey), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> CreateObjective(Guid id, [FromBody] CreatePlanningIntervalObjectiveRequest request, CancellationToken cancellationToken)
    {
        if (id != request.PlanningIntervalId)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(nameof(id), nameof(request.PlanningIntervalId), HttpContext));

        var result = await _sender.Send(request.ToCreatePlanningIntervalObjectiveCommand(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetObjective), new { idOrKey = id, objectiveIdOrKey = result.Value.Id.ToString() }, result.Value)
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPut("{id}/objectives/{objectiveId}")]
    [MustHavePermission(ApplicationAction.Manage, ApplicationResource.PlanningIntervalObjectives)]
    [OpenApiOperation("Update a planning interval objective.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> UpdateObjective(Guid id, Guid objectiveId, [FromBody] UpdatePlanningIntervalObjectiveRequest request, CancellationToken cancellationToken)
    {
        if (id != request.PlanningIntervalId)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(nameof(id), nameof(request.PlanningIntervalId), HttpContext));
        else if (objectiveId != request.ObjectiveId)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(nameof(objectiveId), nameof(request.ObjectiveId), HttpContext));

        var result = await _sender.Send(request.ToUpdatePlanningIntervalObjectiveCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPut("{id}/objectives/order")]
    [MustHavePermission(ApplicationAction.Manage, ApplicationResource.PlanningIntervalObjectives)]
    [OpenApiOperation("Update the order of planning interval objectives.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> UpdateObjectivesOrder(Guid id, [FromBody] UpdatePlanningIntervalObjectivesOrderRequest request, CancellationToken cancellationToken)
    {
        if (id != request.PlanningIntervalId)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(nameof(id), nameof(request.PlanningIntervalId), HttpContext));

        var result = await _sender.Send(new UpdatePlanningIntervalObjectivesOrderCommand(request.PlanningIntervalId, request.Objectives), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpGet("{idOrKey}/objectives/health-report")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervalObjectives)]
    [OpenApiOperation("Get a health report for planning interval objectives.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PlanningIntervalObjectiveHealthCheckDto>>> GetObjectivesHealthReport(string idOrKey, Guid? teamId, CancellationToken cancellationToken)
    {
        var objectives = await _sender.Send(new GetPlanningIntervalObjectivesQuery(idOrKey, teamId), cancellationToken);
        if (objectives == null)
            return Ok(new List<PlanningIntervalObjectiveHealthCheckDto>());

        // get healthchecks
        var healthCheckIds = objectives.Where(o => o.HealthCheck is not null).Select(o => o.HealthCheck!.Id).ToList();
        var healthChecks = await _sender.Send(new GetHealthChecksQuery(healthCheckIds), cancellationToken);

        var objectiveHealthChecks = new List<PlanningIntervalObjectiveHealthCheckDto>(objectives.Count);
        foreach (var objective in objectives)
        {
            var healthCheck = healthChecks.FirstOrDefault(h => h.Id == objective.HealthCheck?.Id);
            objectiveHealthChecks.Add(PlanningIntervalObjectiveHealthCheckDto.Create(objective, healthCheck));
        }

        return Ok(objectiveHealthChecks);
    }

    [HttpGet("{idOrKey}/objectives/{objectiveIdOrKey}/work-items")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervalObjectives)]
    [OpenApiOperation("Get work items for an objective.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkItemsSummaryDto>> GetObjectiveWorkItems(string idOrKey, string objectiveIdOrKey, CancellationToken cancellationToken)
    {
        var objectiveId = await _sender.Send(new CheckPlanningIntervalObjectiveExistsQuery(idOrKey, objectiveIdOrKey), cancellationToken);
        if (objectiveId is null)
            return NotFound();

        var workItemsSummary = await _sender.Send(new GetExternalObjectWorkItemsQuery(objectiveId.Value), cancellationToken);

        if (workItemsSummary is null)
            return NotFound();

        if (workItemsSummary.WorkItems.Count > 0)
            workItemsSummary.WorkItems = [.. workItemsSummary.WorkItems.OrderBy(w => w.StackRank)];

        return Ok(workItemsSummary);
    }

    [HttpGet("{idOrKey}/objectives/{objectiveIdOrKey}/work-items/metrics")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervalObjectives)]
    [OpenApiOperation("Get metrics for the work items linked to an objective.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<WorkItemProgressDailyRollupDto>>> GetObjectiveWorkItemMetrics(string idOrKey, string objectiveIdOrKey, CancellationToken cancellationToken)
    {
        var objectiveId = await _sender.Send(new CheckPlanningIntervalObjectiveExistsQuery(idOrKey, objectiveIdOrKey), cancellationToken);
        if (objectiveId is null)
            return NotFound();

        var planningInterval = await _sender.Send(new GetPlanningIntervalQuery(idOrKey), cancellationToken);

        var today = _dateTimeProvider.Now.ToDateOnly();
        var piEnd = planningInterval!.End.ToDateOnly();
        // get the min of today and the end of the PI
        var end = today < piEnd ? today : piEnd;

        var dailyRollup = await _sender.Send(new GetExternalObjectWorkItemMetricsQuery(objectiveId.Value, planningInterval!.Start.ToDateOnly(), end), cancellationToken);

        if (dailyRollup is null)
            return NotFound();

        return Ok(dailyRollup);
    }

    [HttpPost("{id}/objectives/{objectiveId}/work-items")]
    [MustHavePermission(ApplicationAction.Manage, ApplicationResource.PlanningIntervalObjectives)]
    [OpenApiOperation("Manage objective work items.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> ManageObjectiveWorkItems(Guid id, Guid objectiveId, [FromBody] ManagePlanningIntervalObjectiveWorkItemsRequest request, CancellationToken cancellationToken)
    {
        if (id != request.PlanningIntervalId)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(nameof(id), nameof(request.PlanningIntervalId), HttpContext));
        else if (objectiveId != request.ObjectiveId)
            return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(nameof(objectiveId), nameof(request.ObjectiveId), HttpContext));

        var confirmedObjectiveId = await _sender.Send(new CheckPlanningIntervalObjectiveExistsQuery(new IdOrKey(id), new IdOrKey(objectiveId)), cancellationToken);
        if (confirmedObjectiveId is null)
            return NotFound();

        var result = await _sender.Send(request.ToManageExternalObjectWorkItemsCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPost("{id}/objectives/import")]
    [MustHavePermission(ApplicationAction.Import, ApplicationResource.PlanningIntervalObjectives)]
    [OpenApiOperation("Import objectives for a planning interval from a csv file.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> ImportObjectives(Guid id, [FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        try
        {
            var importedObjectives = _csvService.ReadCsv<ImportPlanningIntervalObjectivesRequest>(file.OpenReadStream());

            List<ImportPlanningIntervalObjectiveDto> objectives = [];
            var validator = new ImportPlanningIntervalObjectivesRequestValidator();
            foreach (var objective in importedObjectives)
            {
                // TODO: allow importing of objectives for multiple PIs at once
                if (id != objective.PlanningIntervalId)
                    return BadRequest(ProblemDetailsExtensions.ForRouteParamMismatch(nameof(id), nameof(objective.PlanningIntervalId), HttpContext));

                var validationResults = await validator.ValidateAsync(objective, cancellationToken);
                if (!validationResults.IsValid)
                {
                    foreach (var error in validationResults.Errors)
                    {
                        if (error.PropertyName != "RecordId")
                        {
                            error.ErrorMessage = $"{error.ErrorMessage} (Record Id: {objective.ImportId})";
                            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                        }
                    }
                    return UnprocessableEntity(validationResults);
                }
                else
                {
                    objectives.Add(objective.ToImportPlanningIntervalObjectiveDto());
                }
            }

            if (!objectives.Any())
                return BadRequest("No PI objectives imported.");

            var result = await _sender.Send(new ImportPlanningIntervalObjectivesCommand(objectives), cancellationToken);

            return result.IsSuccess
                ? NoContent()
                : BadRequest(result.ToBadRequestObject(HttpContext));
        }
        catch (CsvHelperException ex)
        {
            // TODO: Convert this to a validation problem details
            return BadRequest(ProblemDetailsExtensions.ForBadRequest(ex.ToString(), HttpContext));
        }
    }

    [HttpDelete("{id}/objectives/{objectiveId}")]
    [MustHavePermission(ApplicationAction.Manage, ApplicationResource.PlanningIntervalObjectives)]
    [OpenApiOperation("Delete a planning interval objective.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DeleteObjective(Guid id, Guid objectiveId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeletePlanningIntervalObjectiveCommand(id, objectiveId), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpGet("objective-statuses")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervalObjectives)]
    [OpenApiOperation("Get a list of all PI objective statuses.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PlanningIntervalObjectiveStatusDto>>> GetObjectiveStatuses(CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new GetPlanningIntervalObjectiveStatusesQuery(), cancellationToken);
        return Ok(items.OrderBy(c => c.Order));
    }

    #endregion Objectives

    #region Risks

    [HttpGet("{idOrKey}/risks")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervals)]
    [OpenApiOperation("Get planning interval risks. The default value for includeClosed is false.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<RiskListDto>>> GetRisks(string idOrKey, bool? includeClosed, Guid? teamId, CancellationToken cancellationToken)
    {
        includeClosed ??= false;
        var risks = await _sender.Send(new GetRisksByPlanningIntervalQuery(idOrKey, includeClosed.Value, teamId), cancellationToken);

        return Ok(risks);
    }

    #endregion Risks

}
