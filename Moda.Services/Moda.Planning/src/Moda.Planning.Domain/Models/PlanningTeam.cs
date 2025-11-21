using Moda.Common.Domain.Enums.Organization;
using Moda.Common.Domain.Interfaces;
using Moda.Common.Domain.Interfaces.Organization;
using Moda.Common.Domain.Models.Organizations;

namespace Moda.Planning.Domain.Models;

/// <summary>
/// A copy of the Moda.Common.Domain.Interfaces.Organization.ISimpleTeam interface.  Used to hold basic team information for the planning service and db context.
/// </summary>
public class PlanningTeam : ISimpleTeam, IHasIdAndKey
{
    protected readonly List<PlanningIntervalTeam> _planningIntervalTeams = [];

    private PlanningTeam() { }

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
    public IReadOnlyCollection<PlanningIntervalTeam> PlanningIntervalTeams => _planningIntervalTeams.AsReadOnly();

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
