using CSharpFunctionalExtensions;
using Moda.Common.Domain.Enums.Organization;
using Moda.Organization.Domain.Enums;
using NodaTime;

namespace Moda.Organization.Domain.Models;

/// <summary>
/// A team is a collection of team members that work together to execute against a prioritized set of goals.
/// </summary>
/// <seealso cref="Moda.Organization.Domain.Models.BaseTeam" />
/// <seealso cref="Moda.Common.Domain.Interfaces.Organization.IActivatable{NodaTime.Instant, Moda.Organization.Domain.Models.TeamDeactivatableArgs}" />"/>
public sealed class Team : BaseTeam, IActivatable<Instant, TeamDeactivatableArgs>
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
        if (IsActive)
        {
            var membershipStates = ParentMemberships.Select(x => x.StateOn(args.AsOfDate)).ToArray();
            if (membershipStates.Any(m => m == MembershipState.Active || m == MembershipState.Future))
                return Result.Failure("Cannot deactivate a team that has active team memberships.");


            if (args.AsOfDate <= ActiveDate
                || args.AsOfDate < ParentMemberships.Max(x => x.DateRange.End))
                return Result.Failure("Cannot deactivate a team of teams before the active timestamp or before the end of the latest membership.");

            InactiveDate = args.AsOfDate;
            IsActive = false;
            AddDomainEvent(EntityDeactivatedEvent.WithEntity(this, args.Timestamp)); // TODO: this doesn't include the asOfTimestamp
        }

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
