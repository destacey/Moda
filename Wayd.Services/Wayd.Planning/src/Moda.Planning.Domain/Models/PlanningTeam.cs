using Wayd.Common.Domain.Enums.Organization;
using Wayd.Common.Domain.Events.Organization;
using Wayd.Common.Domain.Interfaces;
using Wayd.Common.Domain.Interfaces.Organization;
using Wayd.Common.Domain.Models.Organizations;
using Wayd.Planning.Domain.Models.Iterations;

namespace Wayd.Planning.Domain.Models;

/// <summary>
/// A copy of the Wayd.Common.Domain.Interfaces.Organization.ISimpleTeam interface.  Used to hold team information for the planning service and db context.
/// </summary>
public sealed class PlanningTeam : ISimpleTeam, IHasIdAndKey, IHasTeamIdAndCode
{
    private readonly List<Iteration> _iterations = [];
    private readonly List<PlanningIntervalTeam> _planningIntervalTeams = [];

    private PlanningTeam() { }

    public PlanningTeam(TeamCreatedEvent team)
    {
        Id = team.Id;
        Key = team.Key;
        Name = team.Name;
        Code = team.Code;
        Type = team.Type;
        IsActive = team.IsActive;
    }

    public PlanningTeam(ISimpleTeam team)
    {
        Id = team.Id;
        Key = team.Key;
        Name = team.Name;
        Code = team.Code;
        Type = team.Type;
        IsActive = team.IsActive;
    }

    public Guid Id { get; private set; }
    public int Key { get; private set; }
    public string Name { get; private set; } = default!;
    public TeamCode Code { get; private set; } = default!;
    public TeamType Type { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public IReadOnlyCollection<Iteration> Iterations => _iterations.AsReadOnly();
    public IReadOnlyCollection<PlanningIntervalTeam> PlanningIntervalTeams => _planningIntervalTeams.AsReadOnly();

    /// <summary>
    /// Update the team information based on a TeamUpdatedEvent
    /// </summary>
    /// <param name="team"></param>
    public void Update(TeamUpdatedEvent team)
    {
        Name = team.Name;
        Code = team.Code;
    }

    public void UpdateIsActive(bool isActive)
    {
        IsActive = isActive;
    }


    /// <summary>
    /// Used to resynchronize the PlanningTeam with an ISimpleTeam instance.
    /// </summary>
    /// <param name="team"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void UpdateSimpleTeam(ISimpleTeam team)
    {
        if (Id != team.Id)
        {
            throw new InvalidOperationException("Cannot update PlanningTeam with a different Id.");
        }

        Key = team.Key;
        Name = team.Name;
        Code = team.Code;
        Type = team.Type;
        IsActive = team.IsActive;
    }

    public bool EqualsSimpleTeam(ISimpleTeam? other)
    {
        if (other is null) return false;

        return Id == other.Id
            && Key == other.Key
            && string.Equals(Name, other.Name, StringComparison.Ordinal)
            && Equals(Code, other.Code)
            && Equals(Type, other.Type)
            && IsActive == other.IsActive;
    }
}
