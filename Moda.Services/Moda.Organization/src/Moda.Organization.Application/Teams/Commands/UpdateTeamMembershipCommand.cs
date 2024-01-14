namespace Moda.Organization.Application.Teams.Commands;

public sealed record UpdateTeamMembershipCommand(Guid TeamId, Guid TeamMembershipId, MembershipDateRange DateRange) : ICommand;

public sealed class UpdateTeamMembershipCommandValidator : CustomValidator<UpdateTeamMembershipCommand>
{
    public UpdateTeamMembershipCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.TeamId)
            .NotEmpty();

        RuleFor(t => t.TeamMembershipId)
            .NotEmpty();

        RuleFor(t => t.DateRange)
            .NotNull();
    }
}

internal sealed class UpdateTeamMembershipCommandHandler(IOrganizationDbContext organizationDbContext, IDateTimeProvider dateTimeProvider, ILogger<UpdateTeamMembershipCommandHandler> logger) : ICommandHandler<UpdateTeamMembershipCommand>
{
    private readonly IOrganizationDbContext _organizationDbContext = organizationDbContext;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly ILogger<UpdateTeamMembershipCommandHandler> _logger = logger;

    public async Task<Result> Handle(UpdateTeamMembershipCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Team team = await _organizationDbContext.Teams
                .Include(t => t.ParentMemberships)
                    .ThenInclude(m => m.Target)
                .SingleAsync(t => t.Id == request.TeamId, cancellationToken: cancellationToken);

            var result = team.UpdateTeamMembership(request.TeamMembershipId, request.DateRange, _dateTimeProvider.Now);
            if (result.IsFailure)
                return result;

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