﻿using Moda.Organization.Application.Models;
using Moda.Organization.Application.Teams.Commands;
using Moda.Organization.Application.Teams.Dtos;
using Moda.Organization.Application.Teams.Queries;
using Moda.Organization.Domain.Extensions;
using Moda.Organization.Domain.Models;
using Moda.Planning.Application.Risks.Dtos;
using Moda.Planning.Application.Risks.Queries;
using Moda.Web.Api.Models.Organizations;
using Moda.Web.Api.Models.Organizations.Teams;
using Moda.Web.Api.Models.Planning.Risks;
using Moda.Work.Application.WorkItems.Dtos;
using Moda.Work.Application.WorkItems.Queries;

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
    [OpenApiOperation("Get a list of teams.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesDefaultResponseType(typeof(ErrorResult))]
    public async Task<ActionResult<IReadOnlyList<TeamListDto>>> GetList(CancellationToken cancellationToken, bool includeInactive = false)
    {
        var teams = await _sender.Send(new GetTeamsQuery(includeInactive), cancellationToken);
        return Ok(teams.OrderBy(e => e.Name));
    }

    [HttpGet("{id}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Teams)]
    [OpenApiOperation("Get team details using the key.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType(typeof(ErrorResult))]
    public async Task<ActionResult<TeamDetailsDto>> GetById(int id)
    {
        var team = await _sender.Send(new GetTeamQuery(id));

        return team is not null
            ? Ok(team)
            : NotFound();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.Teams)]
    [OpenApiOperation("Create a team.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.CreateReturn201Int))]
    public async Task<ActionResult> Create([FromBody] CreateTeamRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreateTeamCommand(), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    [HttpPut("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Teams)]
    [OpenApiOperation("Update a team.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateTeamRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
            return BadRequest();

        var result = await _sender.Send(request.ToUpdateTeamCommand(), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.Error);
    }

    //[HttpDelete("{id}")]
    //[MustHavePermission(ApplicationAction.Delete, ApplicationResource.Teams)]
    //[OpenApiOperation("Delete an team.", "")]
    //public async Task<string> Delete(string id)
    //{
    //    throw new NotImplementedException();
    //}

    #region Team Memberships

    [HttpGet("{id}/team-memberships")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Teams)]
    [OpenApiOperation("Get parent team memberships.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<TeamMembershipDto>>> GetTeamMemberships(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _sender.Send(new GetTeamMembershipsQuery(id), cancellationToken));
    }

    [HttpPost("{id}/team-memberships")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Teams)]
    [OpenApiOperation("Add a parent team membership.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> AddTeamMembership(Guid id, [FromBody] AddTeamMembershipRequest request, CancellationToken cancellationToken)
    {
        if (id != request.TeamId)
            return BadRequest();

        var result = await _sender.Send(request.ToTeamAddParentTeamMembershipCommand(), cancellationToken);

        if (result.IsFailure)
        {
            var error = new ErrorResult
            {
                StatusCode = 400,
                SupportMessage = result.Error,
                Source = "TeamsController.AddTeamMembership"
            };
            return BadRequest(error);
        }

        return NoContent();
    }

    [HttpPut("{id}/team-memberships/{teamMembershipId}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Teams)]
    [OpenApiOperation("Update a team membership.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> UpdateTeamMembership(Guid id, Guid teamMembershipId, [FromBody] UpdateTeamMembershipRequest request, CancellationToken cancellationToken)
    {
        if (id != request.TeamId || teamMembershipId != request.TeamMembershipId)
            return BadRequest();

        var result = await _sender.Send(request.ToTeamUpdateTeamMembershipCommand(), cancellationToken);

        if (result.IsFailure)
        {
            var error = new ErrorResult
            {
                StatusCode = 400,
                SupportMessage = result.Error,
                Source = "TeamsController.UpdateTeamMembership"
            };
            return BadRequest(error);
        }

        return NoContent();
    }

    [HttpDelete("{id}/team-memberships/{teamMembershipId}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Teams)]
    [OpenApiOperation("Remove a parent team membership.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RemoveTeamMembership(Guid id, Guid teamMembershipId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RemoveTeamMembershipCommand(id, teamMembershipId), cancellationToken);

        if (result.IsFailure)
        {
            var error = new ErrorResult
            {
                StatusCode = 400,
                SupportMessage = result.Error,
                Source = "TeamsController.RemoveTeamMembership"
            };
            return BadRequest(error);
        }

        return NoContent();
    }

    #endregion Team Memberships

    #region Backlog

    [HttpGet("{idOrCode}/backlog")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.WorkItems)]
    [OpenApiOperation("Get the backlog for a team.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<WorkItemBacklogItemDto>>> GetTeamBacklog(string idOrCode, CancellationToken cancellationToken)
    {
        GetTeamBacklogQuery query;
        if (Guid.TryParse(idOrCode, out Guid guidId))
        {
            query = new GetTeamBacklogQuery(guidId);
        }
        else if (idOrCode.IsValidTeamCodeFormat())
        {
            query = new GetTeamBacklogQuery(new TeamCode(idOrCode));
        }
        else
        {
            return BadRequest(ErrorResult.CreateUnknownIdOrKeyTypeBadRequest("TeamsController.GetTeamBacklog"));
        }

        var result = await _sender.Send(query, cancellationToken);

        return result.IsFailure
            ? BadRequest(ErrorResult.CreateBadRequest(result.Error, "TeamsController.GetTeamBacklog"))
            : Ok(result.Value);
    }

    #endregion Backlog


    #region Risks

    [HttpGet("{id}/risks")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Teams)]
    [OpenApiOperation("Get team risks.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<RiskListDto>>> GetRisks(Guid id, CancellationToken cancellationToken, bool includeClosed = false)
    {
        var teamExists = await _sender.Send(new TeamExistsQuery(id), cancellationToken);
        if (!teamExists)
            return NotFound();

        var risks = await _sender.Send(new GetRisksQuery(id, includeClosed), cancellationToken);

        return Ok(risks);
    }

    [HttpGet("{id}/risks/{riskIdOrKey}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.Teams)]
    [OpenApiOperation("Get a team risk by Id.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RiskDetailsDto>> GetRiskById(Guid id, string riskIdOrKey, CancellationToken cancellationToken)
    {
        var teamExists = await _sender.Send(new TeamExistsQuery(id), cancellationToken);
        if (!teamExists)
            return NotFound();

        var risk = await _sender.Send(new GetRiskQuery(riskIdOrKey), cancellationToken);

        return risk is not null && risk.Team?.Id == id
            ? Ok(risk)
            : NotFound();
    }

    [HttpPost("{id}/risks")]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.Risks)]
    [OpenApiOperation("Create a risk for a team.", "")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> CreateRisk(Guid id, [FromBody] CreateRiskRequest request, CancellationToken cancellationToken)
    {
        if (id != request.TeamId)
            return BadRequest();

        var teamExists = await _sender.Send(new TeamExistsQuery(id), cancellationToken);
        if (!teamExists)
            return NotFound();

        var result = await _sender.Send(request.ToCreateRiskCommand(), cancellationToken);

        if (result.IsFailure)
        {
            var error = new ErrorResult
            {
                StatusCode = 400,
                SupportMessage = result.Error,
                Source = "TeamsController.CreateRisk"
            };
            return BadRequest(error);
        }

        return CreatedAtAction(nameof(GetRiskById), new { id, riskId = result.Value }, result.Value);
    }

    [HttpPut("{id}/risks/{riskId}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.Risks)]
    [OpenApiOperation("Update a team risk.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> UpdateRisk(Guid id, Guid riskId, [FromBody] UpdateRiskRequest request, CancellationToken cancellationToken)
    {
        if (id != request.TeamId || riskId != request.RiskId)
            return BadRequest();

        var result = await _sender.Send(request.ToUpdateRiskCommand(), cancellationToken);

        if (result.IsFailure)
        {
            var error = new ErrorResult
            {
                StatusCode = 400,
                SupportMessage = result.Error,
                Source = "TeamsController.UpdateRisk"
            };
            return BadRequest(error);
        }

        return NoContent();
    }

    #endregion Risks
}
