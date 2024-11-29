using CSharpFunctionalExtensions;
using Moda.Common.Domain.Enums.Organization;
using Moda.Organization.Domain.Enums;
using NodaTime;

namespace Moda.Organization.Domain.Models;

/// <summary>
/// A team of teams is a collection of teams and/or other team of teams that aims to help deliver products collaboratively in the same complex environment.
/// </summary>
/// <seealso cref="Moda.Organization.Domain.Models.BaseTeam" />
/// <seealso cref=""/>
public sealed class TeamOfTeams : BaseTeam, IActivatable<Instant, TeamDeactivatableArgs>
{
    private readonly List<TeamMembership> _childMemberships = [];

    private TeamOfTeams() { }

    private TeamOfTeams(string name, TeamCode code, string? description, LocalDate activeDate)
    {
        Name = name;
        Code = code;
        Description = description;
        Type = TeamType.TeamOfTeams;
        ActiveDate = activeDate;
    }

    public IReadOnlyCollection<TeamMembership> ChildMemberships => _childMemberships.AsReadOnly();

    /// <summary>
    /// The process for activating a team of teams.
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public Result Activate(Instant timestamp)
    {
        if (!IsActive)
        {
            IsActive = true;
            InactiveDate = null;
            AddDomainEvent(EntityActivatedEvent.WithEntity(this, timestamp));
        }

        return Result.Success();
    }

    /// <summary>
    /// The process for deactivating a team of teams.
    /// </summary>
    /// <param name="args"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public Result Deactivate(TeamDeactivatableArgs args)
    {
        if (IsActive)
        {
            var parentMembershipStates = ParentMemberships.Select(x => x.StateOn(args.AsOfDate)).ToArray();
            var childMembershipStates = ParentMemberships.Select(x => x.StateOn(args.AsOfDate)).ToArray();
            if (parentMembershipStates.Union(childMembershipStates).Any(m => m == MembershipState.Active || m == MembershipState.Future))
                return Result.Failure("Cannot deactivate a team of teams that has active team memberships.");

            if (args.AsOfDate <= ActiveDate
                || args.AsOfDate < _parentMemberships.Max(x => x.DateRange.End)
                || args.AsOfDate < _childMemberships.Max(x => x.DateRange.End))
                return Result.Failure("Cannot deactivate a team of teams before the active timestamp or before the end of the latest membership.");

            InactiveDate = args.AsOfDate;
            IsActive = false;
            AddDomainEvent(EntityDeactivatedEvent.WithEntity(this, args.Timestamp)); // TODO: this doesn't include the asOfTimestamp
        }

        return Result.Success();
    }

    /// <summary>Update team of teams.</summary>
    /// <param name="name">The name.</param>
    /// <param name="code">The code.</param>
    /// <param name="description">The description.</param>
    /// <param name="timestamp">The timestamp.</param>
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

    /// <summary>Gets the descendant team ids as of.</summary>
    /// <param name="date">The date.</param>
    /// <param name="includeFuture">if set to <c>true</c> [include future].</param>
    /// <returns></returns>
    public List<Guid> GetDescendantTeamIdsAsOf(LocalDate date, bool includeFuture = false)
    {
        var query = _childMemberships.Where(x => x.StateOn(date) == MembershipState.Active).AsQueryable();
        if (includeFuture)
        {
            query = _childMemberships.Where(x => x.StateOn(date) == MembershipState.Future).AsQueryable();
        }

        List<Guid> descendantTeamIds = new();
        foreach (var membership in _childMemberships)
        {
            if (membership.Source is TeamOfTeams teamOfTeams)
            {
                descendantTeamIds.AddRange(teamOfTeams.GetDescendantTeamIdsAsOf(date, includeFuture));
            }

            descendantTeamIds.Add(membership.SourceId);
        }

        return descendantTeamIds;
    }

    /// <summary>Creates the specified team of teams.</summary>
    /// <param name="name">The name.</param>
    /// <param name="code">The code.</param>
    /// <param name="description">The description.</param>
    /// <param name="activeDate">The active date.</param>
    /// <param name="timestamp">The timestamp.</param>
    /// <returns></returns>
    public static TeamOfTeams Create(string name, TeamCode code, string? description, LocalDate activeDate, Instant timestamp)
    {
        var team = new TeamOfTeams(name, code, description, activeDate);

        team.AddDomainEvent(EntityCreatedEvent.WithEntity(team, timestamp));
        return team;
    }
}
