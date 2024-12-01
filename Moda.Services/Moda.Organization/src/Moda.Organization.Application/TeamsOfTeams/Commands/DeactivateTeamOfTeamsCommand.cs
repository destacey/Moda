using System.Linq.Expressions;
using Moda.Common.Application.Models;
using NodaTime;

namespace Moda.Organization.Application.TeamsOfTeams.Commands;

public sealed record DeactivateTeamOfTeamsCommand : ICommand
{
    public DeactivateTeamOfTeamsCommand(IdOrKey idOrKey, LocalDate inactiveDate)
    {
        IdOrKeyFilter = idOrKey.CreateFilter<TeamOfTeams>();
        InactiveDate = inactiveDate;
    }

    public Expression<Func<TeamOfTeams, bool>> IdOrKeyFilter { get; }
    public LocalDate InactiveDate { get; set; }
}

internal sealed class DeactivateTeamOfTeamsCommandHandler : ICommandHandler<DeactivateTeamOfTeamsCommand>
{
    private const string RequestName = nameof(DeactivateTeamOfTeamsCommand);

    private readonly IOrganizationDbContext _organizationDbContext;
    private readonly ILogger<DeactivateTeamOfTeamsCommand> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DeactivateTeamOfTeamsCommandHandler(IOrganizationDbContext organizationDbContext, ILogger<DeactivateTeamOfTeamsCommand> logger, IDateTimeProvider dateTimeProvider)
    {
        _organizationDbContext = organizationDbContext;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> Handle(DeactivateTeamOfTeamsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var team = await _organizationDbContext.TeamOfTeams
                .FirstOrDefaultAsync(request.IdOrKeyFilter, cancellationToken);

            if (team is null)
            {
                _logger.LogInformation("{RequestName}: Team of Teams not found. {@Request}", RequestName, request);
                return Result.Failure("Team of Teams not found.");
            }

            var result = team.Deactivate(TeamDeactivatableArgs.Create(request.InactiveDate, _dateTimeProvider.Now));
            if (result.IsFailure)
            {
                // Reset the entity
                await _organizationDbContext.Entry(team).ReloadAsync(cancellationToken);
                team.ClearDomainEvents();

                _logger.LogError("{RequestName}: failed to deactivate Team of Teams {TeamId}. Error: {Error}", RequestName, team.Id, result.Error);

                return result;
            }

            await _organizationDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("{RequestName}: deactivated Team of Teams {TeamId}", RequestName, team.Id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception for request {RequestName}: {@Request}", RequestName, request);

            return Result.Failure<int>($"Exception for request {RequestName} {request}");
        }
    }
}