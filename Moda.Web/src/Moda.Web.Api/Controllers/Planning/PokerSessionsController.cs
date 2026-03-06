using Moda.Common.Application.Models;
using Moda.Planning.Application.PokerSessions.Commands;
using Moda.Planning.Application.PokerSessions.Dtos;
using Moda.Planning.Application.PokerSessions.Queries;
using Moda.Planning.Domain.Enums;
using Moda.Web.Api.Extensions;
using Moda.Web.Api.Models.Planning.PokerSessions;

namespace Moda.Web.Api.Controllers.Planning;

[Route("api/planning/poker-sessions")]
[ApiVersionNeutral]
[ApiController]
public class PokerSessionsController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;

    [HttpGet]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PokerSessions)]
    [OpenApiOperation("Get a list of poker sessions.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<PokerSessionListDto>>> GetList(CancellationToken cancellationToken, [FromQuery] PokerSessionStatus? status = null)
    {
        var sessions = await _sender.Send(new GetPokerSessionsQuery(status), cancellationToken);
        return Ok(sessions);
    }

    [HttpGet("{idOrKey}")]
    [MustHavePermission(ApplicationAction.View, ApplicationResource.PokerSessions)]
    [OpenApiOperation("Get poker session details using the Id or key.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PokerSessionDetailsDto>> GetSession(string idOrKey, CancellationToken cancellationToken)
    {
        var session = await _sender.Send(new GetPokerSessionQuery(idOrKey), cancellationToken);
        return session is not null
            ? Ok(session)
            : NotFound();
    }

    [HttpPost]
    [MustHavePermission(ApplicationAction.Create, ApplicationResource.PokerSessions)]
    [OpenApiOperation("Create a poker session.", "")]
    [ApiConventionMethod(typeof(ModaApiConventions), nameof(ModaApiConventions.CreateReturn201IdAndKey))]
    public async Task<ActionResult<ObjectIdAndKey>> Create([FromBody] CreatePokerSessionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToCreatePokerSessionCommand(), cancellationToken);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetSession), new { idOrKey = result.Value.Id.ToString() }, result.Value)
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPut("{id}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.PokerSessions)]
    [OpenApiOperation("Update a poker session.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdatePokerSessionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToUpdatePokerSessionCommand(id), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpDelete("{id}")]
    [MustHavePermission(ApplicationAction.Delete, ApplicationResource.PokerSessions)]
    [OpenApiOperation("Delete a poker session.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeletePokerSessionCommand(id), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPut("{id}/complete")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.PokerSessions)]
    [OpenApiOperation("Complete a poker session.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Complete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CompletePokerSessionCommand(id), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPost("{id}/rounds")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.PokerSessions)]
    [OpenApiOperation("Add a round to a poker session.", "")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PokerRoundDto>> AddRound(Guid id, [FromBody] AddPokerRoundRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToAddPokerRoundCommand(id), cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpDelete("{id}/rounds/{roundId}")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.PokerSessions)]
    [OpenApiOperation("Remove a round from a poker session.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RemoveRound(Guid id, Guid roundId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RemovePokerRoundCommand(id, roundId), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPut("{id}/rounds/{roundId}/reveal")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.PokerSessions)]
    [OpenApiOperation("Reveal votes for a round.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RevealRound(Guid id, Guid roundId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RevealPokerRoundCommand(id, roundId), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPut("{id}/rounds/{roundId}/reset")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.PokerSessions)]
    [OpenApiOperation("Reset a round to allow re-voting.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ResetRound(Guid id, Guid roundId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ResetPokerRoundCommand(id, roundId), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPut("{id}/rounds/{roundId}/consensus")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.PokerSessions)]
    [OpenApiOperation("Set the consensus estimate for a round.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> SetConsensus(Guid id, Guid roundId, [FromBody] SetConsensusRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToSetConsensusCommand(id, roundId), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPut("{id}/rounds/{roundId}/label")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.PokerSessions)]
    [OpenApiOperation("Update the label for a round.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateRoundLabel(Guid id, Guid roundId, [FromBody] UpdatePokerRoundLabelRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToUpdatePokerRoundLabelCommand(id, roundId), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpPost("{id}/rounds/{roundId}/vote")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.PokerSessions)]
    [OpenApiOperation("Submit a vote for a round.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> SubmitVote(Guid id, Guid roundId, [FromBody] SubmitVoteRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(request.ToSubmitVoteCommand(id, roundId), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }

    [HttpDelete("{id}/rounds/{roundId}/vote")]
    [MustHavePermission(ApplicationAction.Update, ApplicationResource.PokerSessions)]
    [OpenApiOperation("Withdraw a vote from a round.", "")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> WithdrawVote(Guid id, Guid roundId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new WithdrawVoteCommand(id, roundId), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : BadRequest(result.ToBadRequestObject(HttpContext));
    }
}
