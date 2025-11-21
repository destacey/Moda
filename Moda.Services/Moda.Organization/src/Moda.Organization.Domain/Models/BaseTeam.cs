using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Domain.Enums.Organization;
using Moda.Common.Domain.Interfaces.Organization;
using Moda.Common.Domain.Models.Organizations;
using Moda.Common.Extensions;
using NodaTime;

namespace Moda.Organization.Domain.Models;
public abstract class BaseTeam : BaseSoftDeletableEntity<Guid>, ISimpleTeam, IHasIdAndKey
{
    private string _name = null!;
    private TeamCode _code = null!;
    private string? _description;

    protected readonly List<TeamMembership> _parentMemberships = [];

    /// <summary>Gets the key.</summary>
    /// <value>The key.</value>
    public int Key { get; protected init; }

    /// <summary>
    /// The name of the team.
    /// </summary>
    public string Name
    {
        get => _name;
        protected set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>Gets the code.</summary>
    /// <value>The code.</value>
    public TeamCode Code
    {
        get => _code;
        protected set => _code = Guard.Against.Null(value, nameof(Code));
    }

    /// <summary>
    /// The description of the team.
    /// </summary>
    public string? Description
    {
        get => _description;
        protected set => _description = value.NullIfWhiteSpacePlusTrim();
    }

    /// <summary>Gets the type.  This value should not change.</summary>
    /// <value>The type.</value>
    public TeamType Type { get; protected set; }

    /// <summary>
    /// The date for when the team became active.
    /// </summary>
    public LocalDate ActiveDate { get; protected set; }

    /// <summary>
    /// The date for when the team became inactive.
    /// </summary>
    public LocalDate? InactiveDate { get; protected set; }

    /// <summary>
    /// Indicates whether the team is active or not.  
    /// </summary>
    public bool IsActive { get; protected set; } = true;

    /// <summary>Gets the parent memberships.</summary>
    /// <value>The parent memberships.</value>
    public IReadOnlyCollection<TeamMembership> ParentMemberships => _parentMemberships.AsReadOnly();

    /// <summary>Adds the team membership.</summary>
    /// <param name="parentTeam">The parent team.</param>
    /// <param name="dateRange">The date range.</param>
    /// <param name="timestamp">The timestamp.</param>
    /// <returns></returns>
    public Result<TeamMembership> AddTeamMembership(TeamOfTeams parentTeam, MembershipDateRange dateRange, Instant timestamp)
    {
        try
        {
            Guard.Against.Null(parentTeam);
            Guard.Against.Null(dateRange);

            if (!IsActive)
                return Result.Failure<TeamMembership>($"Memberships can not be added to inactive teams. {Name} is inactive.");

            if (!parentTeam.IsActive)
                return Result.Failure<TeamMembership>($"Memberships can not be added to inactive teams. {parentTeam.Name} is inactive.");

            if (_parentMemberships.Any(m => m.DateRange.Overlaps(dateRange)))
                return Result.Failure<TeamMembership>("Teams can only have one active parent Team Membership.  This membership would create an overlapping membership.");

            if (this is TeamOfTeams teamOfTeams)
            {
                var descendantIds = teamOfTeams.GetDescendantTeamIdsAsOf(timestamp.InUtc().Date);
                if (descendantIds.Contains(parentTeam.Id))
                    return Result.Failure<TeamMembership>($"The parent team {parentTeam.Name} is a descendant of this team.  This would create a circular reference.");
            }

            var membership = TeamMembership.Create(Id, parentTeam.Id, dateRange);
            _parentMemberships.Add(membership);

            return membership;
        }
        catch (Exception ex)
        {
            return Result.Failure<TeamMembership>(ex.ToString());
        }
    }

    /// <summary>
    /// Updates the team membership.
    /// </summary>
    /// <param name="membershipId"></param>
    /// <param name="dateRange"></param>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public Result<TeamMembership> UpdateTeamMembership(Guid membershipId, MembershipDateRange dateRange, Instant timestamp)
    {
        try
        {
            Guard.Against.Null(dateRange);

            var membership = _parentMemberships.Single(m => m.Id == membershipId);

            if (!IsActive)
                return Result.Failure<TeamMembership>($"Memberships can not be updated on inactive teams. {Name} is inactive.");

            if (!membership.Target.IsActive)
                return Result.Failure<TeamMembership>($"Memberships can not be updated on inactive teams. {membership.Target.IsActive} is inactive.");

            if (_parentMemberships.Any(m => m.Id != membershipId && m.DateRange.Overlaps(dateRange)))
                return Result.Failure<TeamMembership>("Teams can only have one active parent Team Membership.  This membership would create an overlapping membership.");

            membership.Update(dateRange);

            return membership;
        }
        catch (Exception ex)
        {
            return Result.Failure<TeamMembership>(ex.ToString());
        }
    }

    /// <summary>Removes a team membership.</summary>
    /// <param name="membershipId">The membership identifier.</param>
    /// <returns>On success, returns the TeamMembership that was deleted.  This is needed until the EF core bug is fixed.</returns>
    public Result<TeamMembership> RemoveTeamMembership(Guid membershipId)
    {
        try
        {
            var membership = _parentMemberships.Single(m => m.Id == membershipId);

            if (!IsActive)
                return Result.Failure<TeamMembership>($"Memberships can not be removed from inactive teams. {Name} is inactive.");

            if (!membership.Target.IsActive)
                return Result.Failure<TeamMembership>($"Memberships can not be removed from inactive teams. {membership.Target.IsActive} is inactive.");

            _parentMemberships.Remove(membership);

            return Result.Success(membership);
        }
        catch (Exception ex)
        {
            return Result.Failure<TeamMembership>(ex.ToString());
        }
    }
}
