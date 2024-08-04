using Moda.Common.Domain.Enums.Organization;
using Moda.Common.Domain.Interfaces.Organization;

namespace Moda.Work.Domain.Models;

/// <summary>
/// A copy of the Moda.Common.Domain.Interfaces.Organization.ISimpleTeam interface.  Used to hold basic team information for the work service and db context.
/// </summary>
public class WorkTeam : ISimpleTeam
{
    private WorkTeam() { }

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
    public string Code { get; private set; } = default!;
    public TeamType Type { get; private set; } = default!;
    public bool IsActive { get; private set; }

    /// <summary>Updates the specified team from an Organization ISimpleTeam.</summary>
    /// <param name="team">The team or team of teams.</param>
    public void Update(ISimpleTeam team)
    {
        Id = team.Id;
        Key = team.Key;
        Name = team.Name;
        Code = team.Code;
        Type = team.Type;
        IsActive = team.IsActive;
    }
}
