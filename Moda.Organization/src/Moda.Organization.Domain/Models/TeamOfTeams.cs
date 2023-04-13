using CSharpFunctionalExtensions;
using Moda.Organization.Domain.Enums;
using NodaTime;

namespace Moda.Organization.Domain.Models;

/// <summary>
/// A team of teams is a collection of teams and/or other team of teams that aims to help deliver products collaboratively in the same complex environment.
/// </summary>
/// <seealso cref="Moda.Organization.Domain.Models.BaseTeam" />
/// <seealso cref="Moda.Common.Domain.Interfaces.IActivatable" />
public sealed class TeamOfTeams : BaseTeam, IActivatable
{
    private readonly List<TeamMembership> _childMemberships = new();

    private TeamOfTeams() { }

    private TeamOfTeams(string name, TeamCode code, string? description)
    {
        Name = name;
        Code = code;
        Description = description;
        Type = TeamType.TeamOfTeams;
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
        }

        return Result.Success();
    }

    /// <summary>
    /// The process for deactivating a team of teams.
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public Result Deactivate(Instant timestamp)
    {
        if (IsActive)
        {
            var parentMembershipStates = ParentMemberships.Select(x => x.StateOn(timestamp.InUtc().LocalDateTime.Date)).ToArray();
            var childMembershipStates = ParentMemberships.Select(x => x.StateOn(timestamp.InUtc().LocalDateTime.Date)).ToArray();
            if (parentMembershipStates.Union(childMembershipStates).Any(m => m == MembershipState.Active || m == MembershipState.Future))
                return Result.Failure("Cannot deactivate a team of teams that has active team memberships.");

            IsActive = false;
        }

        return Result.Success();
    }

    /// <summary>
    /// Update team of teams.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="code"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    public Result Update(string name, TeamCode code, string? description)
    {
        try
        {
            Name = name;
            Code = code;
            Description = description;

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    /// <summary>Creates the specified team of teams.</summary>
    /// <param name="name">The name.</param>
    /// <param name="code">The code.</param>
    /// <param name="description">The description.</param>
    /// <returns></returns>
    public static TeamOfTeams Create(string name, TeamCode code, string? description)
    {
        return new TeamOfTeams(name, code, description);
    }
}
