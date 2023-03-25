using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Moda.Organization.Application.TeamsOfTeams.Commands;
public sealed record UpdateTeamOfTeamsCommand : ICommand<int>
{
    public UpdateTeamOfTeamsCommand(Guid id, string name, TeamCode code, string? description)
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

public sealed class UpdateTeamOfTeamsCommandValidator : CustomValidator<UpdateTeamOfTeamsCommand>
{
    private readonly IOrganizationDbContext _organizationDbContext;

    public UpdateTeamOfTeamsCommandValidator(IOrganizationDbContext organizationDbContext)
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

internal sealed class UpdateTeamOfTeamsCommandHandler : ICommandHandler<UpdateTeamOfTeamsCommand, int>
{
    private readonly IOrganizationDbContext _organizationDbContext;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<UpdateTeamOfTeamsCommandHandler> _logger;

    public UpdateTeamOfTeamsCommandHandler(IOrganizationDbContext organizationDbContext, IDateTimeService dateTimeService, ILogger<UpdateTeamOfTeamsCommandHandler> logger)
    {
        _organizationDbContext = organizationDbContext;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(UpdateTeamOfTeamsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var team = await _organizationDbContext.TeamOfTeams
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
            if (team is null)
                return Result.Failure<int>("Team of Teams not found.");

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

