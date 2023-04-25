using Moda.Organization.Application.Models;
using Moda.Organization.Application.TeamsOfTeams.Commands;
using Moda.Organization.Application.TeamsOfTeams.Dtos;
using Moda.Organization.Application.TeamsOfTeams.Queries;
using Moda.Web.Api.Models.Organizations;
using Moda.Web.Api.Models.Organizations.TeamOfTeams;
using Moda.Web.Api.Models.Organizations.TeamsOfTeams;

namespace Moda.Web.Api.Controllers.Organizations;

[Route("api/organization/teams-of-teams")]
[ApiVersionNeutral]
[ApiController]
public class TeamsOfTeamsController : ControllerBase
{
    private readonly ILogger<TeamsOfTeamsController> _logger;
    private readonly ISender _sender;

    public TeamsOfTeamsController(ILogger<TeamsOfTeamsController> logger, ISender sender)
    {
        _logger = logger;
        _sender = sender;
    }

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Teams)]
    [OpenApiOperation("Get a list of team of teams.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesDefaultResponseType(typeof(ErrorResult))]
    public async Task<ActionResult<IReadOnlyList<TeamOfTeamsListDto>>> GetList(CancellationToken cancellationToken, bool includeInactive = false)
    {
        var teams = await _sender.Send(new GetTeamOfTeamsListQuery(includeInactive), cancellationToken);
        return Ok(teams.OrderBy(e => e.Name));
    }

    [HttpGet("{id}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Teams)]
    [OpenApiOperation("Get team of teams details using the localId.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType(typeof(ErrorResult))]
    public async Task<ActionResult<TeamOfTeamsDetailsDto>> GetById(int id)
    {
        var team = await _sender.Send(new GetTeamOfTeamsQuery(id));

        return team is not null
            ? Ok(team)
            : NotFound();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.Teams)]
    [OpenApiOperation("Create a team of teams.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.Create))]
    public async Task<ActionResult> Create(CreateTeamOfTeamsRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateTeamOfTeamsCommand(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPut("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Teams)]
    [OpenApiOperation("Update an team.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(HttpValidationProblemDetails))]
    [ProducesDefaultResponseType(typeof(ErrorResult))]
    public async Task<ActionResult> Update(Guid id, UpdateTeamOfTeamsRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest();

        var result = await _sender.Send(request.ToUpdateTeamOfTeamsCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.Error);
    }

    //[HttpDelete("{id}")]
    //[MustHavePermission(ApplicationAction.Delete, ApplicationResource.TeamsOfTeams)]
    //[OpenApiOperation("Delete an team.", "")]
    //public async Task<string> Delete(string id)
    //{
    //    throw new NotImplementedException();
    //}

    [HttpGet("{id}/team-memberships")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Teams)]
    [OpenApiOperation("Get parent team memberships.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    [ProducesDefaultResponseType(typeof(ErrorResult))]
    public async Task<ActionResult<IReadOnlyList<TeamMembershipsDto>>> GetTeamMemberships(Guid id, CancellationToken cancellationToken)
    {
        var memberships = await _sender.Send(new GetTeamMembershipsQuery(id), cancellationToken);

        return Ok(memberships);
    }

    [HttpPost("{id}/team-memberships")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Teams)]
    [OpenApiOperation("Add a parent team membership.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(HttpValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    [ProducesDefaultResponseType(typeof(ErrorResult))]
    public async Task<ActionResult> AddTeamMembership(Guid id, [FromBody] AddTeamMembershipRequest request, CancellationToken cancellationToken)
    {
        if (id != request.TeamId)
            return BadRequest();

        var result = await _sender.Send(request.ToTeamOfTeamsAddParentTeamMembershipCommand(), cancellationToken);

        if (result.IsFailure)
        {
            var error = new ErrorResult
            {
                StatusCode = 400,
                SupportMessage = result.Error,
                Source = "TeamsOfTeamsController.AddTeamMembership"
            };
            return BadRequest(error);
        }

        return NoContent();
    }

    [HttpPut("{id}/team-memberships/{teamMembershipId}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Teams)]
    [OpenApiOperation("Update a team membership.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(HttpValidationProblemDetails))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    [ProducesDefaultResponseType(typeof(ErrorResult))]
    public async Task<ActionResult> UpdateTeamMembership(Guid id, Guid teamMembershipId, [FromBody] UpdateTeamMembershipRequest request, CancellationToken cancellationToken)
    {
        if (id != request.TeamId || teamMembershipId != request.TeamMembershipId)
            return BadRequest();

        var result = await _sender.Send(request.ToTeamOfTeamsUpdateTeamMembershipCommand(), cancellationToken);

        if (result.IsFailure)
        {
            var error = new ErrorResult
            {
                StatusCode = 400,
                SupportMessage = result.Error,
                Source = "TeamsOfTeamsController.UpdateTeamMembership"
            };
            return BadRequest(error);
        }

        return NoContent();
    }

    [HttpDelete("{id}/team-memberships/{teamMembershipId}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Teams)]
    [OpenApiOperation("Remove a parent team membership.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResult))]
    [ProducesDefaultResponseType(typeof(ErrorResult))]
    public async Task<ActionResult> RemoveTeamMembership(Guid id, Guid teamMembershipId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RemoveTeamMembershipCommand(id, teamMembershipId), cancellationToken);

        if (result.IsFailure)
        {
            var error = new ErrorResult
            {
                StatusCode = 400,
                SupportMessage = result.Error,
                Source = "TeamsOfTeamsController.RemoveTeamMembership"
            };
            return BadRequest(error);
        }

        return NoContent();
    }
}
