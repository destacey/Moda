﻿using CSharpFunctionalExtensions;
using Moda.Organization.Domain.Enums;
using NodaTime;

namespace Moda.Organization.Domain.Models;

/// <summary>
/// A team is a collection of team members that work together to execute against a prioritized set of goals.
/// </summary>
/// <seealso cref="Moda.Organization.Domain.Models.BaseTeam" />
/// <seealso cref="Moda.Common.Domain.Interfaces.IActivatable" />
public sealed class Team : BaseTeam, IActivatable
{
    private Team() { }

    private Team(string name, TeamCode code, string? description, TeamType type)
    {
        Name = name;
        Code = code;
        Description = description;
        Type = type;
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
        }

        return Result.Success();
    }

    /// <summary>
    /// The process for deactivating an organization.
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns>Result that indicates success or a list of errors</returns>
    public Result Deactivate(Instant timestamp)
    {
        if (IsActive)
        {
            // TODO is there logic that would prevent deactivation?
            var membershipStates = ParentMemberships.Select(x => x.StateOn(timestamp.InUtc().LocalDateTime.Date)).ToArray();
            if (membershipStates.Any(m => m == MembershipState.Active || m == MembershipState.Future))
                return Result.Failure("Cannot deactivate a team that has active team to team memberships.");

            IsActive = false;
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

    /// <summary>Creates the specified team.</summary>
    /// <param name="name">The name.</param>
    /// <param name="code">The code.</param>
    /// <param name="description">The description.</param>
    /// <returns></returns>
    public static Team Create(string name, TeamCode code, string? description)
    {
        return new Team(name, code, description, TeamType.Team);
    }
}
