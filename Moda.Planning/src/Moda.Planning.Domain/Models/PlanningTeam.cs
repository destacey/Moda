using Moda.Common.Domain.Interfaces;
using Moda.Organization.Domain.Enums;
using Moda.Organization.Domain.Models;
using NodaTime;

namespace Moda.Planning.Domain.Models;

/// <summary>
/// A copy of the Moda.Organization.Domain.Models.Team class.  Used to hold basic team information for the planning service and db context.
/// </summary>
/// <seealso cref="Moda.Common.Domain.Data.BaseEntity&lt;System.Guid&gt;" />
/// <seealso cref="Moda.Common.Domain.Interfaces.ISoftDelete" />
public class PlanningTeam : BaseEntity<Guid>, ISoftDelete
{
    protected readonly List<ProgramIncrementTeam> _programIncrementTeams = new();

    private PlanningTeam() { }

    public PlanningTeam(BaseTeam team)
    {
        Id = team.Id;
        Key = team.Key;
        Name = team.Name;
        Code = team.Code;
        Type = team.Type;
        IsActive = team.IsActive;
        Deleted = team.Deleted;
        DeletedBy = team.DeletedBy;
        IsDeleted = team.IsDeleted;
    }

    public int Key { get; private set; }
    public string Name { get; private set; } = default!;
    public TeamCode Code { get; private set; } = default!;
    public TeamType Type { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public Instant? Deleted { get; set; }
    public Guid? DeletedBy { get; set; }
    public bool IsDeleted { get; set; }
    public IReadOnlyCollection<ProgramIncrementTeam> ProgramIncrementTeams => _programIncrementTeams.AsReadOnly();


    /// <summary>Updates the specified team from an Organization BaseTeam.</summary>
    /// <param name="team">The team or team of teams.</param>
    public void Update(BaseTeam team)
    {
        Id = team.Id;
        Key = team.Key;
        Name = team.Name;
        Code = team.Code;
        Type = team.Type;
        IsActive = team.IsActive;
        Deleted = team.Deleted;
        DeletedBy = team.DeletedBy;
        IsDeleted = team.IsDeleted;
    }
}
