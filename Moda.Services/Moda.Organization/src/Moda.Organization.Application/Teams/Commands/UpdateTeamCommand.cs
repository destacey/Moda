namespace Moda.Organization.Application.Teams.Commands;
public sealed record UpdateTeamCommand : ICommand<int>
{
    public UpdateTeamCommand(Guid id, string name, TeamCode code, string? description)
    {
        Id = id;
        Name = name;
        Code = code;
        Description = description;
    }

    /// <summary>Gets or sets the identifier.</summary>
    /// <value>The identifier.</value>
    public Guid Id { get; }

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

public sealed class UpdateTeamCommandValidator : CustomValidator<UpdateTeamCommand>
{
    private readonly IOrganizationDbContext _organizationDbContext;

    public UpdateTeamCommandValidator(IOrganizationDbContext organizationDbContext)
    {
        _organizationDbContext = organizationDbContext;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.Name)
            .NotEmpty()
            .MaximumLength(128)
            .MustAsync(async (model, name, cancellationToken) =>
            {
                return await BeUniqueTeamName(model.Id, name, cancellationToken);
            }).WithMessage("The Team name already exists.");

        RuleFor(t => t.Code)
            .NotEmpty()
            .SetValidator(t => new TeamCodeValidator(_organizationDbContext, t.Id));

        RuleFor(t => t.Description)
            .MaximumLength(1024);
    }

    public async Task<bool> BeUniqueTeamName(Guid id, string name, CancellationToken cancellationToken)
    {
        return await _organizationDbContext.BaseTeams.Where(t => t.Id != id).AllAsync(x => x.Name != name, cancellationToken);
    }
}

internal sealed class UpdateTeamCommandHandler : ICommandHandler<UpdateTeamCommand, int>
{
    private readonly IOrganizationDbContext _organizationDbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<UpdateTeamCommandHandler> _logger;

    public UpdateTeamCommandHandler(IOrganizationDbContext organizationDbContext, IDateTimeService dateTimeService, ILogger<UpdateTeamCommandHandler> logger)
    {
        _organizationDbContext = organizationDbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(UpdateTeamCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var team = await _organizationDbContext.Teams
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
            if (team is null)
                return Result.Failure<int>("Team not found.");

            var updateResult = team.Update(
                request.Name,
                request.Code,
                request.Description, 
                _dateTimeService.Now
                );

            if (updateResult.IsFailure)
            {
                // Reset the entity
                await _organizationDbContext.Entry(team).ReloadAsync(cancellationToken);
                team.ClearDomainEvents();

                var requestName = request.GetType().Name;
                _logger.LogError("Moda Request: Failure for Request {Name} {@Request}.  Error message: {Error}", requestName, request, updateResult.Error);
                return Result.Failure<int>(updateResult.Error);
            }

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

