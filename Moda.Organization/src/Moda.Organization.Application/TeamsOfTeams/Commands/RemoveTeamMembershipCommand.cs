namespace Moda.Organization.Application.TeamsOfTeams.Commands;

public sealed record RemoveTeamMembershipCommand(Guid TeamId, Guid TeamMembershipId) : ICommand;

public sealed class RemoveTeamMembershipCommandValidator : CustomValidator<RemoveTeamMembershipCommand>
{
    public RemoveTeamMembershipCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.TeamId)
            .NotEmpty();

        RuleFor(t => t.TeamMembershipId)
            .NotEmpty();
    }
}

internal sealed class RemoveTeamMembershipCommandHandler : ICommandHandler<RemoveTeamMembershipCommand>
{
    private readonly IOrganizationDbContext _organizationDbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<RemoveTeamMembershipCommandHandler> _logger;

    public RemoveTeamMembershipCommandHandler(IOrganizationDbContext organizationDbContext, IDateTimeService dateTimeService, ILogger<RemoveTeamMembershipCommandHandler> logger)
    {
        _organizationDbContext = organizationDbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<Result> Handle(RemoveTeamMembershipCommand request, CancellationToken cancellationToken)
    {
        try
        {
            TeamOfTeams team = await _organizationDbContext.TeamOfTeams
                .Include(t => t.ParentMemberships.Where(m => m.Id == request.TeamMembershipId))
                    .ThenInclude(m => m.Target)
                .AsNoTracking() // needed until the EF Core bug below is fixed
                .SingleAsync(t => t.Id == request.TeamId);

            var result = team.RemoveTeamMembership(request.TeamMembershipId);
            if (result.IsFailure)
                return result;

            /// Cleans up deleted team memberships.  This is needed because of a bug in EF Core 7.x.
            _organizationDbContext.Entry(result.Value).State = EntityState.Deleted;

            await _organizationDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}