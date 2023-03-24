using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Moda.Organization.Application.TeamsOfTeams.Commands;
public sealed record CreateTeamOfTeamsCommand : ICommand<int>
{
    public CreateTeamOfTeamsCommand(string name, TeamCode code, string? description)
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

public sealed class CreateTeamOfTeamsCommandValidator : CustomValidator<CreateTeamOfTeamsCommand>
{
    private readonly IOrganizationDbContext _organizationDbContext;

    public CreateTeamOfTeamsCommandValidator(IOrganizationDbContext organizationDbContext)
    {
        _organizationDbContext = organizationDbContext;

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.Name)
            .SetValidator(t => new TeamNameValidator(_organizationDbContext));

        RuleFor(t => t.Code)
            .NotEmpty()
            .SetValidator(new TeamCodeValidator(_organizationDbContext));

        RuleFor(t => t.Description)
            .MaximumLength(1024);
    }
}

internal sealed class CreateTeamOfTeamsCommandHandler : ICommandHandler<CreateTeamOfTeamsCommand, int>
{
    private readonly IOrganizationDbContext _organizationDbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<CreateTeamOfTeamsCommandHandler> _logger;

    public CreateTeamOfTeamsCommandHandler(IOrganizationDbContext organizationDbContext, IDateTimeService dateTimeService, ILogger<CreateTeamOfTeamsCommandHandler> logger)
    {
        _organizationDbContext = organizationDbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(CreateTeamOfTeamsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var team = TeamOfTeams.Create(request.Name, request.Code, request.Description, _dateTimeService.Now);
            await _organizationDbContext.TeamOfTeams.AddAsync(team, cancellationToken);

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
