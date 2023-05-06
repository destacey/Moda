namespace Moda.Organization.Application.Teams.Commands;
public sealed record AddTeamMembershipCommand : ICommand
{
    public AddTeamMembershipCommand(Guid teamId, Guid parentTeamId, MembershipDateRange dateRange)
    {
        TeamId = teamId;
        ParentTeamId = parentTeamId;
        DateRange = dateRange;
    }

    public Guid TeamId { get; }
    public Guid ParentTeamId { get; }
    public MembershipDateRange DateRange { get; }
}

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

internal sealed class AddTeamMembershipCommandHandler : ICommandHandler<AddTeamMembershipCommand>
{
    private readonly IOrganizationDbContext _organizationDbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<AddTeamMembershipCommandHandler> _logger;

    public AddTeamMembershipCommandHandler(IOrganizationDbContext organizationDbContext, IDateTimeService dateTimeService, ILogger<AddTeamMembershipCommandHandler> logger)
    {
        _organizationDbContext = organizationDbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<Result> Handle(AddTeamMembershipCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var team = await _organizationDbContext.Teams
                .Include(t => t.ParentMemberships)
                .SingleAsync(t => t.Id == request.TeamId);

            var parentTeam = await _organizationDbContext.TeamOfTeams
                .SingleAsync(t => t.Id == request.ParentTeamId);

            var result = team.AddTeamMembership(parentTeam, request.DateRange, _dateTimeService.Now);
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