using Moda.Common.Domain.Enums.Organization;
using Moda.Common.Domain.Events.Organization;
using Moda.Common.Domain.Interfaces.Organization;
using Moda.Common.Domain.Models.Organizations;

namespace Moda.Work.Domain.Models;

/// <summary>
/// A copy of the Moda.Common.Domain.Interfaces.Organization.ISimpleTeam interface.  Used to hold basic team information for the work service and db context.
/// </summary>
public class WorkTeam : ISimpleTeam, IHasIdAndKey, IHasTeamIdAndCode
{
    private WorkTeam() { }

    public WorkTeam(TeamCreatedEvent team)
    {
        Id = team.Id;
        Key = team.Key;
        Name = team.Name;
        Code = team.Code;
        Type = team.Type;
        IsActive = team.IsActive;
    }

    public WorkTeam(ISimpleTeam team)
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
    /// Used to resynchronize the WorkTeam with an ISimpleTeam instance.
    /// </summary>
    /// <param name="team"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void UpdateSimpleTeam(ISimpleTeam team)
    {
        if (Id != team.Id)
        {
            throw new InvalidOperationException("Cannot update WorkTeam with a different Id.");
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
