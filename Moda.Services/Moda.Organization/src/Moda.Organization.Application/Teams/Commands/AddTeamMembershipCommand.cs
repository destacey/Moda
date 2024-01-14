namespace Moda.Organization.Application.Teams.Commands;
public sealed record AddTeamMembershipCommand(Guid TeamId, Guid ParentTeamId, MembershipDateRange DateRange) : ICommand;

public sealed class AddTeamMembershipCommandValidator : CustomValidator<AddTeamMembershipCommand>
{
    public AddTeamMembershipCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.TeamId)
            .NotEmpty();

        RuleFor(t => t.ParentTeamId)
            .NotEmpty();

        RuleFor(t => t.DateRange)
            .NotNull();
    }
}

internal sealed class AddTeamMembershipCommandHandler(IOrganizationDbContext organizationDbContext, IDateTimeProvider dateTimeProvider, ILogger<AddTeamMembershipCommandHandler> logger) : ICommandHandler<AddTeamMembershipCommand>
{
    private readonly IOrganizationDbContext _organizationDbContext = organizationDbContext;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly ILogger<AddTeamMembershipCommandHandler> _logger = logger;

    public async Task<Result> Handle(AddTeamMembershipCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var team = await _organizationDbContext.Teams
                .Include(t => t.ParentMemberships)
                .SingleAsync(t => t.Id == request.TeamId, cancellationToken: cancellationToken);

            var parentTeam = await _organizationDbContext.TeamOfTeams
                .SingleAsync(t => t.Id == request.ParentTeamId, cancellationToken: cancellationToken);

            var result = team.AddTeamMembership(parentTeam, request.DateRange, _dateTimeProvider.Now);
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