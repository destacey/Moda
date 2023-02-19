using Moda.Organization.Application.Teams.Dtos;
using Moda.Organization.Application.Teams.Queries;

namespace Moda.Web.Api.Controllers.Organizations;

public class TeamsController : VersionNeutralApiController
{
    private readonly ILogger<TeamsController> _logger;
    private readonly ISender _sender;

    public TeamsController(ILogger<TeamsController> logger, ISender sender)
    {
        _logger = logger;
        _sender = sender;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Teams)]
    [OpenApiOperation("Get a list of all teams.", "")]
    public async Task<ActionResult<IReadOnlyList<TeamListDto>>> GetList(CancellationToken cancellationToken, bool includeInactive = false)
    {
        var teams = await _sender.Send(new GetTeamsQuery(includeInactive), cancellationToken);
        return Ok(teams.OrderBy(e => e.Name));
    }

    [HttpGet("{id}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Teams)]
    [OpenApiOperation("Get team details using the localId.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeamDetailsDto>> GetById(int id)
    {
        var team = await _sender.Send(new GetTeamQuery(id));

        return team is not null
            ? Ok(team)
            : NotFound();
    }

    //[HttpPost]
    //[MustHavePermission(ApplicationAction.Create, ApplicationResource.Teams)]
    //[OpenApiOperation("Create an team.", "")]
    //[ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.Create))]
    //public async Task<ActionResult> Create(CreateTeamRequest request, CancellationToken cancellationToken)
    //{
    //    var result = await _sender.Send(request.ToCreateTeamCommand(), cancellationToken);

    //    return result.IsSuccess
    //        ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
    //        : BadRequest(result.Error);
    //}

    //[HttpPut("{id}")]
    //[MustHavePermission(ApplicationAction.Update, ApplicationResource.Teams)]
    //[OpenApiOperation("Update an team.", "")]
    //[ProducesResponseType(StatusCodes.Status204NoContent)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //public async Task<ActionResult> Update(Guid id, UpdateTeamRequest request, CancellationToken cancellationToken)
    //{
    //    if (id != request.Id)
    //        return BadRequest();

    //    var result = await _sender.Send(request.ToUpdateTeamCommand(), cancellationToken);

    //    return result.IsSuccess
    //        ? NoContent()
    //        : BadRequest(result.Error);
    //}

    //[HttpDelete("{id}")]
    //[MustHavePermission(ApplicationAction.Delete, ApplicationResource.Teams)]
    //[OpenApiOperation("Delete an team.", "")]
    //public async Task<string> Delete(string id)
    //{
    //    throw new NotImplementedException();
    //}
}
