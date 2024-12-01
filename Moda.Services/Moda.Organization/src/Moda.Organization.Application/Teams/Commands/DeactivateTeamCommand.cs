using System.Linq.Expressions;
using Moda.Common.Application.Models;
using NodaTime;

namespace Moda.Organization.Application.Teams.Commands;
public sealed record DeactivateTeamCommand : ICommand
{
    public DeactivateTeamCommand(IdOrKey idOrKey, LocalDate inactiveDate)
    {
        IdOrKeyFilter = idOrKey.CreateFilter<Team>();
        InactiveDate = inactiveDate;
    }

    public Expression<Func<Team, bool>> IdOrKeyFilter { get; }
    public LocalDate InactiveDate { get; set; }
}

internal sealed class DeactivateTeamCommandHandler : ICommandHandler<DeactivateTeamCommand>
{
    private const string RequestName = nameof(DeactivateTeamCommand);

    private readonly IOrganizationDbContext _organizationDbContext;
    private readonly ILogger<DeactivateTeamCommand> _logger;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DeactivateTeamCommandHandler(IOrganizationDbContext organizationDbContext, ILogger<DeactivateTeamCommand> logger, IDateTimeProvider dateTimeProvider)
    {
        _organizationDbContext = organizationDbContext;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> Handle(DeactivateTeamCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var team = await _organizationDbContext.Teams
                .FirstOrDefaultAsync(request.IdOrKeyFilter, cancellationToken);

            if (team is null)
            {
                _logger.LogInformation("{RequestName}: Team not found. {@Request}", RequestName, request);
                return Result.Failure("Team not found.");
            }

            var result = team.Deactivate(TeamDeactivatableArgs.Create(request.InactiveDate, _dateTimeProvider.Now));
            if (result.IsFailure)
            {
                // Reset the entity
                await _organizationDbContext.Entry(team).ReloadAsync(cancellationToken);
                team.ClearDomainEvents();

                _logger.LogError("{RequestName}: failed to deactivate Team {TeamId}. Error: {Error}", RequestName, team.Id, result.Error);

                return result;
            }

            await _organizationDbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("{RequestName}: deactivated Team {TeamId}", RequestName, team.Id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception for request {RequestName}: {@Request}", RequestName, request);

            return Result.Failure<int>($"Exception for request {RequestName} {request}");
        }
    }
}
