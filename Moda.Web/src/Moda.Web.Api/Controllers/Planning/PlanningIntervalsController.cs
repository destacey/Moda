using CsvHelper;
using Mapster;
using Moda.Common.Application.Interfaces;
using Moda.Health.Queries;
using Moda.Organization.Application.Teams.Queries;
using Moda.Organization.Application.TeamsOfTeams.Queries;
using Moda.Planning.Application.PlanningIntervals.Commands;
using Moda.Planning.Application.PlanningIntervals.Dtos;
using Moda.Planning.Application.PlanningIntervals.Queries;
using Moda.Planning.Application.Risks.Dtos;
using Moda.Planning.Application.Risks.Queries;
using Moda.Web.Api.Dtos.Planning;
using Moda.Web.Api.Models.Planning.PlanningIntervals;

namespace Moda.Web.Api.Controllers.Planning;

[Route("api/planning/planning-intervals")]
[ApiVersionNeutral]
[ApiController]
public class PlanningIntervalsController : ControllerBase
{
    private readonly ILogger<PlanningIntervalsController> _logger;
    private readonly ISender _sender;
    private readonly ICsvService _csvService;

    public PlanningIntervalsController(ILogger<PlanningIntervalsController> logger, ISender sender, ICsvService csvService)
    {
        _logger = logger;
        _sender = sender;
        _csvService = csvService;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervals)]
    [OpenApiOperation("Get a list of planning intervals.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PlanningIntervalListDto>>> GetList(CancellationToken cancellationToken)
    {
        var planningIntervals = await _sender.Send(new GetPlanningIntervalsQuery(), cancellationToken);
        return Ok(planningIntervals);
    }

    [HttpGet("{id}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervals)]
    [OpenApiOperation("Get planning interval details.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType(typeof(ErrorResult))]
    public async Task<ActionResult<PlanningIntervalDetailsDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var planningInterval = await _sender.Send(new GetPlanningIntervalQuery(id), cancellationToken);

        return planningInterval is not null
            ? Ok(planningInterval)
            : NotFound();
    }

    [HttpGet("key/{id}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervals)]
    [OpenApiOperation("Get planning interval details using the key.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType(typeof(ErrorResult))]
    public async Task<ActionResult<PlanningIntervalDetailsDto>> GetByKey(int id, CancellationToken cancellationToken)
    {
        var planningInterval = await _sender.Send(new GetPlanningIntervalQuery(id), cancellationToken);

        return planningInterval is not null
            ? Ok(planningInterval)
            : NotFound();
    }

    [HttpGet("{id}/predictability")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervals)]
    [OpenApiOperation("Get the PI predictability for all teams.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PlanningIntervalPredictabilityDto>> GetPredictability(Guid id, CancellationToken cancellationToken)
    {
        var predictability = await _sender.Send(new GetPlanningIntervalPredictabilityQuery(id), cancellationToken);

        return predictability is not null
            ? Ok(predictability)
            : NotFound();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.PlanningIntervals)]
    [OpenApiOperation("Create a planning interval.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.CreateReturn201Guid))]
    public async Task<ActionResult> Create([FromBody] CreatePlanningIntervalRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreatePlanningIntervalCommand(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPut("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.PlanningIntervals)]
    [OpenApiOperation("Update a planning interval.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdatePlanningIntervalRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest();

        var result = await _sender.Send(request.ToUpdatePlanningIntervalCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.Error);
    }

    [HttpGet("{id}/teams")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervals)]
    [OpenApiOperation("Get a list of planning interval teams.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PlanningIntervalTeamResponse>>> GetTeams(Guid id, CancellationToken cancellationToken)
    {
        List<PlanningIntervalTeamResponse> piTeams = new();
        var teamIds = await _sender.Send(new GetPlanningIntervalTeamsQuery(id), cancellationToken);

        if (teamIds.Any())
        {
            var teams = await _sender.Send(new GetTeamsQuery(true, teamIds), cancellationToken);
            var teamOfTeams = await _sender.Send(new GetTeamOfTeamsListQuery(true, teamIds), cancellationToken);

            piTeams.AddRange(teams.Adapt<List<PlanningIntervalTeamResponse>>());
            piTeams.AddRange(teamOfTeams.Adapt<List<PlanningIntervalTeamResponse>>());
        }

        return Ok(piTeams);
    }

    [HttpGet("{id}/teams/{teamId}/predictability")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervals)]
    [OpenApiOperation("Get the PI predictability for a team.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<double?>> GetTeamPredictability(Guid id, Guid teamId, CancellationToken cancellationToken)
    {
        var predictability = await _sender.Send(new GetTeamPlanningIntervalPredictabilityQuery(id, teamId), cancellationToken);

        return Ok(predictability);
    }

    [HttpPost("{id}/teams")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.PlanningIntervals)]
    [OpenApiOperation("Manager planning interval teams.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ManageTeams(Guid id, [FromBody] ManagePlanningIntervalTeamsRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest();

        var result = await _sender.Send(request.ToManagePlanningIntervalTeamsCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.Error);
    }

    #region Objectives

    [HttpGet("{id}/objectives")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervalObjectives)]
    [OpenApiOperation("Get a list of planning interval teams.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PlanningIntervalObjectiveListDto>>> GetObjectives(Guid id, Guid? teamId, CancellationToken cancellationToken)
    {
        var objectives = await _sender.Send(new GetPlanningIntervalObjectivesQuery(id, teamId), cancellationToken);

        return Ok(objectives);
    }

    [HttpGet("{id}/objectives/{objectiveId}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervalObjectives)]
    [OpenApiOperation("Get a planning interval objective.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlanningIntervalObjectiveDetailsDto>> GetObjectiveById(Guid id, Guid objectiveId, CancellationToken cancellationToken)
    {
        var objective = await _sender.Send(new GetPlanningIntervalObjectiveQuery(id, objectiveId), cancellationToken);

        return objective is not null
            ? Ok(objective)
            : NotFound();
    }

    [HttpGet("key/{id}/objectives/{objectiveId}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervalObjectives)]
    [OpenApiOperation("Get a planning interval objective using the PI and Objective keys.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlanningIntervalObjectiveDetailsDto>> GetObjectiveByKey(int id, int objectiveId, CancellationToken cancellationToken)
    {
        var objective = await _sender.Send(new GetPlanningIntervalObjectiveQuery(id, objectiveId), cancellationToken);

        return objective is not null
            ? Ok(objective)
            : NotFound();
    }

    [HttpPost("{id}/objectives")]
    [MustHavePermission(ApplicationAction.Manage, ApplicationResource.PlanningIntervalObjectives)]
    [OpenApiOperation("Create a planning interval objective.", "")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> CreateObjective(Guid id, [FromBody] CreatePlanningIntervalObjectiveRequest request, CancellationToken cancellationToken)
    {
        if (id != request.PlanningIntervalId)
            return BadRequest();

        var result = await _sender.Send(request.ToCreatePlanningIntervalObjectiveCommand(), cancellationToken);

        if (result.IsFailure)
        {
            var error = new ErrorResult
            {
                StatusCode = 400,
                SupportMessage = result.Error,
                Source = "PlanningIntervalsController.CreateObjective"
            };
            return BadRequest(error);
        }

        return CreatedAtAction(nameof(GetObjectiveByKey), new { id, objectiveId = result.Value }, result.Value);
    }

    [HttpPut("{id}/objectives/{objectiveId}")]
    [MustHavePermission(ApplicationAction.Manage, ApplicationResource.PlanningIntervalObjectives)]
    [OpenApiOperation("Update a planning interval objective.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> UpdateObjective(Guid id, Guid objectiveId, [FromBody] UpdatePlanningIntervalObjectiveRequest request, CancellationToken cancellationToken)
    {
        if (id != request.PlanningIntervalId || objectiveId != request.ObjectiveId)
            return BadRequest();

        var result = await _sender.Send(request.ToUpdatePlanningIntervalObjectiveCommand(), cancellationToken);

        if (result.IsFailure)
        {
            var error = new ErrorResult
            {
                StatusCode = 400,
                SupportMessage = result.Error,
                Source = "PlanningIntervalsController.UpdateObjective"
            };
            return BadRequest(error);
        }

        return NoContent();
    }

    [HttpGet("{id}/objectives/health-report")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervalObjectives)]
    [OpenApiOperation("Get a health report for planning interval objectives.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PlanningIntervalObjectiveHealthCheckDto>>> GetObjectivesHealthReport(string id, Guid? teamId, CancellationToken cancellationToken)
    {
        GetPlanningIntervalObjectivesQuery objectivesQuery;
        if (Guid.TryParse(id, out Guid guidId))
        {
            objectivesQuery = new GetPlanningIntervalObjectivesQuery(guidId, teamId);
        }
        else if (int.TryParse(id, out int intId))
        {
            objectivesQuery = new GetPlanningIntervalObjectivesQuery(intId, teamId);
        }
        else
        {
            return NotFound();
        }

        var objectives = await _sender.Send(objectivesQuery, cancellationToken);
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

    [HttpPost("{id}/objectives/import")]
    [MustHavePermission(ApplicationAction.Import, ApplicationResource.PlanningIntervalObjectives)]
    [OpenApiOperation("Import objectives for a planning interval from a csv file.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> ImportObjectives(Guid id, [FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        try
        {
            var importedObjectives = _csvService.ReadCsv<ImportPlanningIntervalObjectivesRequest>(file.OpenReadStream());

            List<ImportPlanningIntervalObjectiveDto> objectives = new();
            var validator = new ImportPlanningIntervalObjectivesRequestValidator();
            foreach (var objective in importedObjectives)
            {
                // TODO: allow importing of objectives for multiple PIs at once
                if (id != objective.PlanningIntervalId)
                    return BadRequest();

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

            if (result.IsFailure)
            {
                var error = new ErrorResult
                {
                    StatusCode = 400,
                    SupportMessage = result.Error,
                    Source = "PlanningIntervalsController.ImportObjectives"
                };
                return BadRequest(error);
            }

            return NoContent();
        }
        catch (CsvHelperException ex)
        {
            // TODO: Convert this to a validation problem details
            var error = new ErrorResult
            {
                StatusCode = 400,
                SupportMessage = ex.ToString(),
                Source = "PlanningIntervalsController.ImportObjectives"
            };
            return BadRequest(error);
        }
    }

    [HttpDelete("{id}/objectives/{objectiveId}")]
    [MustHavePermission(ApplicationAction.Manage, ApplicationResource.PlanningIntervalObjectives)]
    [OpenApiOperation("Delete a planning interval objective.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DeleteObjective(Guid id, Guid objectiveId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeletePlanningIntervalObjectiveCommand(id, objectiveId), cancellationToken);

        if (result.IsFailure)
        {
            var error = new ErrorResult
            {
                StatusCode = 400,
                SupportMessage = result.Error,
                Source = "PlanningIntervalsController.DeleteObjective"
            };
            return BadRequest(error);
        }

        return NoContent();
    }

    [HttpGet("objective-statuses")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervalObjectives)]
    [OpenApiOperation("Get a list of all PI objective statuses.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PlanningIntervalObjectiveStatusDto>>> GetObjectiveStatuses(CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new GetPlanningIntervalObjectiveStatusesQuery(), cancellationToken);
        return Ok(items.OrderBy(c => c.Order));
    }

    #endregion Objectives

    #region Risks

    [HttpGet("{id}/risks")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PlanningIntervals)]
    [OpenApiOperation("Get planning interval risks.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<RiskListDto>>> GetRisks(Guid id, CancellationToken cancellationToken, Guid? teamId = null, bool includeClosed = false)
    {
        var risks = await _sender.Send(new GetRisksByPlanningIntervalQuery(id, includeClosed, teamId), cancellationToken);

        return Ok(risks);
    }

    #endregion Risks
}
