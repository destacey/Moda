using NodaTime;

namespace Moda.Organization.Application.TeamsOfTeams.Commands;
public sealed record CreateTeamOfTeamsCommand(string Name, TeamCode Code, string? Description, LocalDate ActiveDate) : ICommand<int>;

public sealed class CreateTeamOfTeamsCommandValidator : CustomValidator<CreateTeamOfTeamsCommand>
{
    private readonly IOrganizationDbContext _organizationDbContext;

    public CreateTeamOfTeamsCommandValidator(IOrganizationDbContext organizationDbContext)
    {
        _organizationDbContext = organizationDbContext;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.Name)
            .NotEmpty()
            .MaximumLength(128)
            .MustAsync(BeUniqueTeamName).WithMessage("The Team name already exists.");

        RuleFor(t => t.Code)
            .NotEmpty()
            .SetValidator(new TeamCodeValidator(_organizationDbContext));

        RuleFor(t => t.Description)
            .MaximumLength(1024);

        RuleFor(t => t.ActiveDate)
            .NotEmpty();
    }

    public async Task<bool> BeUniqueTeamName(string name, CancellationToken cancellationToken)
    {
        return await _organizationDbContext.BaseTeams.AllAsync(x => x.Name != name, cancellationToken);
    }
}

internal sealed class CreateTeamOfTeamsCommandHandler : ICommandHandler<CreateTeamOfTeamsCommand, int>
{
    private readonly IOrganizationDbContext _organizationDbContext;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<CreateTeamOfTeamsCommandHandler> _logger;

    public CreateTeamOfTeamsCommandHandler(IOrganizationDbContext organizationDbContext, IDateTimeProvider dateTimeProvider, ILogger<CreateTeamOfTeamsCommandHandler> logger)
    {
        _organizationDbContext = organizationDbContext;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(CreateTeamOfTeamsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var team = TeamOfTeams.Create(request.Name, request.Code, request.Description, request.ActiveDate, _dateTimeProvider.Now);
            await _organizationDbContext.TeamOfTeams.AddAsync(team, cancellationToken);

            await _organizationDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(team.Key);
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<int>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}
