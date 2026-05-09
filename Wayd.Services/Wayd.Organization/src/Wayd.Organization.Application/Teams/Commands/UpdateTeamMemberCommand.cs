namespace Wayd.Organization.Application.Teams.Commands;

public sealed record UpdateTeamMemberCommand(Guid TeamId, Guid TeamMemberId, Guid RoleId) : ICommand;

public sealed class UpdateTeamMemberCommandValidator : CustomValidator<UpdateTeamMemberCommand>
{
    public UpdateTeamMemberCommandValidator()
    {
        RuleFor(c => c.TeamId).NotEmpty();
        RuleFor(c => c.TeamMemberId).NotEmpty();
        RuleFor(c => c.RoleId).NotEmpty();
    }
}

internal sealed class UpdateTeamMemberCommandHandler(
    IOrganizationDbContext organizationDbContext,
    ILogger<UpdateTeamMemberCommandHandler> logger)
    : ICommandHandler<UpdateTeamMemberCommand>
{
    private const string AppRequestName = nameof(UpdateTeamMemberCommand);

    private readonly IOrganizationDbContext _organizationDbContext = organizationDbContext;
    private readonly ILogger<UpdateTeamMemberCommandHandler> _logger = logger;

    public async Task<Result> Handle(UpdateTeamMemberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var team = await _organizationDbContext.BaseTeams
                .Include(t => t.Members)
                .SingleOrDefaultAsync(t => t.Id == request.TeamId, cancellationToken);

            if (team is null)
            {
                _logger.LogInformation("Team with Id {TeamId} not found.", request.TeamId);
                return Result.Failure("Team not found.");
            }

            var result = team.UpdateMember(request.TeamMemberId, request.RoleId);
            if (result.IsFailure)
            {
                _logger.LogError("Error updating team member {TeamMemberId} on team {TeamId}. Error message: {Error}", request.TeamMemberId, request.TeamId, result.Error);
                return result;
            }

            await _organizationDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Team member {TeamMemberId} updated on team {TeamId}.", request.TeamMemberId, request.TeamId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception handling {CommandName} command for request {@Request}.", AppRequestName, request);
            return Result.Failure($"Error handling {AppRequestName} command.");
        }
    }
}
