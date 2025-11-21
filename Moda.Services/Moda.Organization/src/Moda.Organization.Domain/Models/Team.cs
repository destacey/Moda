using CSharpFunctionalExtensions;
using Moda.Common.Domain.Enums.Organization;
using Moda.Common.Domain.Models.Organizations;
using Moda.Common.Domain.Interfaces.Organization;
using NodaTime;

namespace Moda.Organization.Domain.Models;

/// <summary>
/// A team is a collection of team members that work together to execute against a prioritized set of goals.
/// </summary>
/// <seealso cref="Moda.Organization.Domain.Models.BaseTeam" />
/// <seealso cref="Moda.Common.Domain.Interfaces.Organization.IActivatable{NodaTime.Instant, Moda.Organization.Domain.Models.TeamDeactivatableArgs}" />"/>
public sealed class Team : BaseTeam, IActivatable<Instant, TeamDeactivatableArgs>, IHasTeamIdAndCode
{
    private Team() { }

    private Team(string name, TeamCode code, string? description, LocalDate activeDate)
    {
        Name = name;
        Code = code;
        Description = description;
        Type = TeamType.Team;
        ActiveDate = activeDate;
    }

    /// <summary>
    /// The process for activating an organization.
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public Result Activate(Instant timestamp)
    {
        if (!IsActive)
        {
            // TODO is there logic that would prevent activation?
            IsActive = true;
            InactiveDate = null;
            AddDomainEvent(EntityActivatedEvent.WithEntity(this, timestamp));
        }

        return Result.Success();
    }

    /// <summary>
    /// The process for deactivating an organization.
    /// </summary>
    /// <param name="args"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public Result Deactivate(TeamDeactivatableArgs args)
    {
        if (!IsActive)
        {
            return Result.Failure("The team is already inactive.");
        }

        if (args.AsOfDate <= ActiveDate)
        {
            return Result.Failure("The inactive date cannot be on or before the active date.");
        }

        if (ParentMemberships.Count != 0)
        {
            var latestMembership = ParentMemberships.OrderByDescending(x => x.DateRange.Start).First();
            if (latestMembership.DateRange.End == null || args.AsOfDate < latestMembership.DateRange.End.Value)
            {
                return Result.Failure("The inactive date must be on or after the end date of the last team membership.");
            }
        }

        InactiveDate = args.AsOfDate;
        IsActive = false;  // TODO: this will be invalid if the InactiveDate is in the future
        AddDomainEvent(EntityDeactivatedEvent.WithEntity(this, args.Timestamp)); // TODO: this doesn't include the asOfTimestamp

        return Result.Success();
    }

    /// <summary>
    /// Update team
    /// </summary>
    /// <param name="name"></param>
    /// <param name="code"></param>
    /// <param name="description"></param>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public Result Update(string name, TeamCode code, string? description, Instant timestamp)
    {
        try
        {
            Name = name;
            Code = code;
            Description = description;

            AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    /// <summary>Creates the specified team.</summary>
    /// <param name="name">The name.</param>
    /// <param name="code">The code.</param>
    /// <param name="description">The description.</param>
    /// <param name="activeDate">The active timestamp.</param>
    /// <param name="timestamp">The timestamp.</param>
    /// <returns></returns>
    public static Team Create(string name, TeamCode code, string? description, LocalDate activeDate, Instant timestamp)
    {
        var team = new Team(name, code, description, activeDate);

        team.AddDomainEvent(EntityCreatedEvent.WithEntity(team, timestamp));
        return team;
    }
}
