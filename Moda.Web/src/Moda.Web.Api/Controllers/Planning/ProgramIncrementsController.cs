using CsvHelper;
using Mapster;
using Moda.Common.Application.Interfaces;
using Moda.Organization.Application.Teams.Queries;
using Moda.Organization.Application.TeamsOfTeams.Queries;
using Moda.Planning.Application.ProgramIncrements.Commands;
using Moda.Planning.Application.ProgramIncrements.Dtos;
using Moda.Planning.Application.ProgramIncrements.Queries;
using Moda.Planning.Application.Risks.Dtos;
using Moda.Planning.Application.Risks.Queries;
using Moda.Web.Api.Models.Planning.ProgramIncrements;

namespace Moda.Web.Api.Controllers.Planning;

[Route("api/planning/program-increments")]
[ApiVersionNeutral]
[ApiController]
public class ProgramIncrementsController : ControllerBase
{
    private readonly ILogger<ProgramIncrementsController> _logger;
    private readonly ISender _sender;
    private readonly ICsvService _csvService;

    public ProgramIncrementsController(ILogger<ProgramIncrementsController> logger, ISender sender, ICsvService csvService)
    {
        _logger = logger;
        _sender = sender;
        _csvService = csvService;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.ProgramIncrements)]
    [OpenApiOperation("Get a list of program increments.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ProgramIncrementListDto>>> GetList(CancellationToken cancellationToken)
    {
        var programIncrements = await _sender.Send(new GetProgramIncrementsQuery(), cancellationToken);
        return Ok(programIncrements);
    }

    [HttpGet("{id}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.ProgramIncrements)]
    [OpenApiOperation("Get program increment details.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType(typeof(ErrorResult))]
    public async Task<ActionResult<ProgramIncrementDetailsDto>> GetById(Guid id)
    {
        var programIncrement = await _sender.Send(new GetProgramIncrementQuery(id));

        return programIncrement is not null
            ? Ok(programIncrement)
            : NotFound();
    }

    [HttpGet("local-id/{id}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.ProgramIncrements)]
    [OpenApiOperation("Get program increment details using the localId.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType(typeof(ErrorResult))]
    public async Task<ActionResult<ProgramIncrementDetailsDto>> GetByLocalId(int id)
    {
        var programIncrement = await _sender.Send(new GetProgramIncrementQuery(id));

        return programIncrement is not null
            ? Ok(programIncrement)
            : NotFound();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.ProgramIncrements)]
    [OpenApiOperation("Create a program increment.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.CreateReturn201Guid))]
    public async Task<ActionResult> Create([FromBody] CreateProgramIncrementRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateProgramIncrementCommand(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPut("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.ProgramIncrements)]
    [OpenApiOperation("Update a program increment.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateProgramIncrementRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest();

        var result = await _sender.Send(request.ToUpdateProgramIncrementCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.Error);
    }

    [HttpGet("{id}/teams")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.ProgramIncrements)]
    [OpenApiOperation("Get a list of program increment teams.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ProgramIncrementTeamResponse>>> GetTeams(Guid id, CancellationToken cancellationToken)
    {
        List<ProgramIncrementTeamResponse> piTeams = new();
        var teamIds = await _sender.Send(new GetProgramIncrementTeamsQuery(id), cancellationToken);
        
        if (teamIds.Any())
        {
            var teams = await _sender.Send(new GetTeamsQuery(true, teamIds), cancellationToken);
            var teamOfTeams = await _sender.Send(new GetTeamOfTeamsListQuery(true, teamIds), cancellationToken);

            piTeams.AddRange(teams.Adapt<List<ProgramIncrementTeamResponse>>());
            piTeams.AddRange(teamOfTeams.Adapt<List<ProgramIncrementTeamResponse>>());
        }

        return Ok(piTeams);
    }

    [HttpPost("{id}/teams")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.ProgramIncrements)]
    [OpenApiOperation("Manager program increment teams.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ManageTeams(Guid id, [FromBody] ManageProgramIncrementTeamsRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest();

        var result = await _sender.Send(request.ToManageProgramIncrementTeamsCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.Error);
    }

    #region Objectives

    [HttpGet("{id}/objectives")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.ProgramIncrementObjectives)]
    [OpenApiOperation("Get a list of program increment teams.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ProgramIncrementObjectiveListDto>>> GetObjectives(Guid id, Guid? teamId, CancellationToken cancellationToken)
    {
        var objectives = await _sender.Send(new GetProgramIncrementObjectivesQuery(id, teamId), cancellationToken);

        return Ok(objectives);
    }

    [HttpGet("{id}/objectives/{objectiveId}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.ProgramIncrementObjectives)]
    [OpenApiOperation("Get a program increment objective.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProgramIncrementObjectiveDetailsDto>> GetObjectiveById(Guid id, Guid objectiveId, CancellationToken cancellationToken)
    {
        var objective = await _sender.Send(new GetProgramIncrementObjectiveQuery(id, objectiveId), cancellationToken);

        return objective is not null
            ? Ok(objective)
            : NotFound();
    }

    [HttpGet("local-id/{id}/objectives/{objectiveId}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.ProgramIncrementObjectives)]
    [OpenApiOperation("Get a program increment objective using the PI and Objective local Ids.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProgramIncrementObjectiveDetailsDto>> GetObjectiveByLocalId(int id, int objectiveId, CancellationToken cancellationToken)
    {
        var objective = await _sender.Send(new GetProgramIncrementObjectiveQuery(id, objectiveId), cancellationToken);

        return objective is not null
            ? Ok(objective)
            : NotFound();
    }

    [HttpPost("{id}/objectives")]
    [MustHavePermission(ApplicationAction.Manage, ApplicationResource.ProgramIncrementObjectives)]
    [OpenApiOperation("Create a program increment objective.", "")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> CreateObjective(Guid id, [FromBody] CreateProgramIncrementObjectiveRequest request, CancellationToken cancellationToken)
    {
        if (id != request.ProgramIncrementId)
            return BadRequest();

        var result = await _sender.Send(request.ToCreateProgramIncrementObjectiveCommand(), cancellationToken);

        if (result.IsFailure)
        {
            var error = new ErrorResult
            {
                StatusCode = 400,
                SupportMessage = result.Error,
                Source = "ProgramIncrementsController.CreateObjective"
            };
            return BadRequest(error);
        }

        return CreatedAtAction(nameof(GetObjectiveByLocalId), new { id, objectiveId = result.Value }, result.Value);
    }

    [HttpPut("{id}/objectives/{objectiveId}")]
    [MustHavePermission(ApplicationAction.Manage, ApplicationResource.ProgramIncrementObjectives)]
    [OpenApiOperation("Update a program increment objective.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> UpdateObjective(Guid id, Guid objectiveId, [FromBody] UpdateProgramIncrementObjectiveRequest request, CancellationToken cancellationToken)
    {
        if (id != request.ProgramIncrementId || objectiveId != request.ObjectiveId)
            return BadRequest();

        var result = await _sender.Send(request.ToUpdateProgramIncrementObjectiveCommand(), cancellationToken);

        if (result.IsFailure)
        {
            var error = new ErrorResult
            {
                StatusCode = 400,
                SupportMessage = result.Error,
                Source = "ProgramIncrementsController.UpdateObjective"
            };
            return BadRequest(error);
        }

        return NoContent();
    }

    [HttpPost("{id}/objectives/import")]
    [MustHavePermission(ApplicationAction.Import, ApplicationResource.ProgramIncrementObjectives)]
    [OpenApiOperation("Import objectives for a program increment from a csv file.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> ImportObjectives(Guid id, [FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        try
        {
            var importedObjectives = _csvService.ReadCsv<ImportProgramIncrementObjectivesRequest>(file.OpenReadStream());

            List<ImportProgramIncrementObjectiveDto> objectives = new();
            var validator = new ImportProgramIncrementObjectivesRequestValidator();
            foreach (var objective in importedObjectives)
            {
                // TODO: allow importing of objectives for multiple PIs at once
                if (id != objective.ProgramIncrementId)
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
                    objectives.Add(objective.ToImportProgramIncrementObjectiveDto());
                }
            }

            if (!objectives.Any())
                return BadRequest("No PI objectives imported.");

            var result = await _sender.Send(new ImportProgramIncrementObjectivesCommand(objectives), cancellationToken);

            if (result.IsFailure)
            {
                var error = new ErrorResult
                {
                    StatusCode = 400,
                    SupportMessage = result.Error,
                    Source = "ProgramIncrementsController.ImportObjectives"
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
                Source = "ProgramIncrementsController.ImportObjectives"
            };
            return BadRequest(error);
        }
    }

    [HttpGet("objective-statuses")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.ProgramIncrementObjectives)]
    [OpenApiOperation("Get a list of all PI objective statuses.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ProgramIncrementObjectiveStatusDto>>> GetObjectiveStatuses(CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new GetProgramIncrementObjectiveStatusesQuery(), cancellationToken);
        return Ok(items.OrderBy(c => c.Order));
    }

    #endregion Objectives

    #region Risks

    [HttpGet("{id}/risks")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.ProgramIncrements)]
    [OpenApiOperation("Get program increment risks.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<RiskListDto>>> GetRisks(Guid id, CancellationToken cancellationToken, bool includeClosed = false)
    {
        var risks = await _sender.Send(new GetRisksByProgramIncrementQuery(id, includeClosed), cancellationToken);

        return Ok(risks);
    }

    #endregion Risks
}
