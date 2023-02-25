using Moda.Organization.Application.Teams.Dtos;
using Moda.Organization.Application.Teams.Queries;
using Moda.Web.Api.Models.Organizations.Teams;

namespace Moda.Web.Api.Controllers.Organizations;

[Route("api/organization/teams")]
[ApiVersionNeutral]
[ApiController]
public class TeamsController : ControllerBase
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

    [HttpPost("/team")]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.Teams)]
    [OpenApiOperation("Create a team.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.Create))]
    public async Task<ActionResult> CreateTeam(CreateTeamRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateTeamCommand(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPost("/team-of-teams")]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.Teams)]
    [OpenApiOperation("Create a team of teams.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.Create))]
    public async Task<ActionResult> CreateTeamOfTeams(CreateTeamOfTeamsRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateTeamOfTeamsCommand(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

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
