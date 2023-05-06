using Moda.Planning.Application.ProgramIncrements.Dtos;
using Moda.Planning.Application.ProgramIncrements.Queries;
using Moda.Web.Api.Models.Planning.ProgramIncrements;

namespace Moda.Web.Api.Controllers.Planning;

[Route("api/planning/program-increments")]
[ApiVersionNeutral]
[ApiController]
public class ProgramIncrementsController : ControllerBase
{
    private readonly ILogger<ProgramIncrementsController> _logger;
    private readonly ISender _sender;

    public ProgramIncrementsController(ILogger<ProgramIncrementsController> logger, ISender sender)
    {
        _logger = logger;
        _sender = sender;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.ProgramIncrements)]
    [OpenApiOperation("Get a list of program increments.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesDefaultResponseType(typeof(ErrorResult))]
    public async Task<ActionResult<IReadOnlyList<ProgramIncrementListDto>>> GetList(CancellationToken cancellationToken)
    {
        var programIncrements = await _sender.Send(new GetProgramIncrementsQuery(), cancellationToken);
        return Ok(programIncrements.OrderByDescending(e => e.Start));
    }

    [HttpGet("{id}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.ProgramIncrements)]
    [OpenApiOperation("Get program increment details using the localId.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesDefaultResponseType(typeof(ErrorResult))]
    public async Task<ActionResult<ProgramIncrementDetailsDto>> GetById(int id)
    {
        var programIncrement = await _sender.Send(new GetProgramIncrementQuery(id));

        return programIncrement is not null
            ? Ok(programIncrement)
            : NotFound();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.ProgramIncrements)]
    [OpenApiOperation("Create a program increment.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.Create))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(HttpValidationProblemDetails))]
    [ProducesDefaultResponseType(typeof(ErrorResult))]
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
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(HttpValidationProblemDetails))]
    [ProducesDefaultResponseType(typeof(ErrorResult))]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateProgramIncrementRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest();

        var result = await _sender.Send(request.ToUpdateProgramIncrementCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.Error);
    }
}
