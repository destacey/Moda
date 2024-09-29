using CsvHelper;
using Moda.Common.Application.Interfaces;
using Moda.Planning.Application.Risks.Commands;
using Moda.Planning.Application.Risks.Dtos;
using Moda.Planning.Application.Risks.Queries;
using Moda.Web.Api.Models.Planning.Risks;

namespace Moda.Web.Api.Controllers.Planning;
[Route("api/planning/risks")]
[ApiVersionNeutral]
[ApiController]
public class RisksController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICsvService _csvService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RisksController(ISender sender, ICsvService csvService, IDateTimeProvider dateTimeProvider)
    {
        _sender = sender;
        _csvService = csvService;
        _dateTimeProvider = dateTimeProvider;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Risks)]
    [OpenApiOperation("Get a list of risks.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesDefaultResponseType(typeof(ErrorResult))]
    public async Task<ActionResult<IReadOnlyList<RiskListDto>>> GetList(CancellationToken cancellationToken, bool includeClosed = false)
    {
        var risks = await _sender.Send(new GetRisksQuery(includeClosed), cancellationToken);
        return Ok(risks);
    }

    [HttpGet("{idOrKey}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Risks)]
    [OpenApiOperation("Get risk details by Id.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType(typeof(ErrorResult))]
    public async Task<ActionResult<RiskDetailsDto>> GetRisk(string idOrKey, CancellationToken cancellationToken)
    {
        var risk = await _sender.Send(new GetRiskQuery(idOrKey), cancellationToken);

        return risk is not null
            ? Ok(risk)
            : NotFound();
    }

    [HttpGet("me")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Risks)]
    [OpenApiOperation("Get a list of open risks assigned to me.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesDefaultResponseType(typeof(ErrorResult))]
    public async Task<ActionResult<IReadOnlyList<RiskListDto>>> GetMyRisks(CancellationToken cancellationToken)
    {
        var risks = await _sender.Send(new GetMyRisksQuery(), cancellationToken);
        return Ok(risks);
    }

    [HttpPost()]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.Risks)]
    [OpenApiOperation("Create a risk.", "")]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> CreateRisk([FromBody] CreateRiskRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateRiskCommand(), cancellationToken);

        if (result.IsFailure)
        {
            var error = new ErrorResult
            {
                StatusCode = 400,
                SupportMessage = result.Error,
                Source = "RisksController.Create"
            };
            return BadRequest(error);
        }

        return CreatedAtAction(nameof(GetRisk), new { idOrKey = result.Value.Id.ToString() }, result.Value);
    }

    [HttpPut("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Risks)]
    [OpenApiOperation("Update a risk.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateRiskRequest request, CancellationToken cancellationToken)
    {
        if (id != request.RiskId)
            return BadRequest();

        var result = await _sender.Send(request.ToUpdateRiskCommand(), cancellationToken);

        if (result.IsFailure)
        {
            var error = new ErrorResult
            {
                StatusCode = 400,
                SupportMessage = result.Error,
                Source = "RisksController.Update"
            };
            return BadRequest(error);
        }

        return NoContent();
    }

    [HttpPost("import")]
    [MustHavePermission(ApplicationAction.Import, ApplicationResource.Risks)]
    [OpenApiOperation("Import risks from a csv file.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Import([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        try
        {
            var importedRisks = _csvService.ReadCsv<ImportRiskRequest>(file.OpenReadStream());

            List<ImportRiskDto> risks = new();
            var validator = new ImportRiskRequestValidator(_dateTimeProvider);
            foreach (var risk in importedRisks)
            {
                var validationResults = await validator.ValidateAsync(risk, cancellationToken);
                if (!validationResults.IsValid)
                {
                    foreach (var error in validationResults.Errors)
                    {
                        if (error.PropertyName != "RecordId")
                        {
                            error.ErrorMessage = $"{error.ErrorMessage} (Record Id: {risk.ImportId})";
                            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                        }
                    }
                    return UnprocessableEntity(validationResults);
                }
                else
                {
                    risks.Add(risk.ToImportRiskDto());
                }
            }

            if (!risks.Any())
                return BadRequest("No risks imported.");

            var result = await _sender.Send(new ImportRisksCommand(risks), cancellationToken);

            if (result.IsFailure)
            {
                var error = new ErrorResult
                {
                    StatusCode = 400,
                    SupportMessage = result.Error,
                    Source = "RisksController.Import"
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
                Source = "RisksController.Import"
            };
            return BadRequest(error);
        }
    }

    [HttpGet("statuses")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Risks)]
    [OpenApiOperation("Get a list of all risk statuses.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<RiskStatusDto>>> GetStatuses(CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new GetRiskStatusesQuery(), cancellationToken);
        return Ok(items.OrderBy(c => c.Order));
    }

    [HttpGet("categories")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Risks)]
    [OpenApiOperation("Get a list of all risk categories.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<RiskCategoryDto>>> GetCategories(CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new GetRiskCategoriesQuery(), cancellationToken);
        return Ok(items.OrderBy(c => c.Order));
    }

    [HttpGet("grades")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Risks)]
    [OpenApiOperation("Get a list of all risk grades.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<RiskGradeDto>>> GetGrades(CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new GetRiskGradesQuery(), cancellationToken);
        return Ok(items.OrderBy(c => c.Order));
    }
}
