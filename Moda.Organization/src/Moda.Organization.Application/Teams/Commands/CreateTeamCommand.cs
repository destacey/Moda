namespace Moda.Organization.Application.Teams.Commands;
public sealed record CreateTeamCommand : ICommand<int>
{
    public CreateTeamCommand(string name, TeamCode code, string? description)
    {
        Name = name;
        Code = code;
        Description = description;
    }

    /// <summary>Gets the team name.</summary>
    /// <value>The team name.</value>
    public string Name { get; }

    /// <summary>Gets the code.</summary>
    /// <value>The code.</value>
    public TeamCode Code { get; }

    /// <summary>Gets the team description.</summary>
    /// <value>The team description.</value>
    public string? Description { get; }
}

public sealed class CreateTeamCommandValidator : CustomValidator<CreateTeamCommand>
{
    private readonly IOrganizationDbContext _organizationDbContext;

    public CreateTeamCommandValidator(IOrganizationDbContext organizationDbContext)
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
    }

    public async Task<bool> BeUniqueTeamName(string name, CancellationToken cancellationToken)
    {
        return await _organizationDbContext.BaseTeams.AllAsync(x => x.Name != name, cancellationToken);
    }
}

internal sealed class CreateTeamCommandHandler : ICommandHandler<CreateTeamCommand, int>
{
    private readonly IOrganizationDbContext _organizationDbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<CreateTeamCommandHandler> _logger;

    public CreateTeamCommandHandler(IOrganizationDbContext organizationDbContext, IDateTimeService dateTimeService, ILogger<CreateTeamCommandHandler> logger)
    {
        _organizationDbContext = organizationDbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(CreateTeamCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var team = Team.Create(request.Name, request.Code, request.Description);
            await _organizationDbContext.Teams.AddAsync((Team)team, cancellationToken);

            await _organizationDbContext.SaveChangesAsync(cancellationToken);

            return Result.Success(team.LocalId);
        }
        catch (Exception ex)
        {
            var requestName = request.GetType().Name;

            _logger.LogError(ex, "Moda Request: Exception for Request {Name} {@Request}", requestName, request);

            return Result.Failure<int>($"Moda Request: Exception for Request {requestName} {request}");
        }
    }
}
