using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;

namespace Moda.ProjectPortfolioManagement.Domain.Models;

/// <summary>
/// Represents a phase definition within a project lifecycle template.
/// </summary>
public sealed class ProjectLifecyclePhase : BaseAuditableEntity<Guid>
{
    private ProjectLifecyclePhase() { }

    internal ProjectLifecyclePhase(Guid projectLifecycleId, string name, string description, int order)
    {
        Id = Guid.CreateVersion7();
        ProjectLifecycleId = projectLifecycleId;
        Name = name;
        Description = description;
        Order = order;
    }

    /// <summary>
    /// The ID of the lifecycle this phase belongs to.
    /// </summary>
    public Guid ProjectLifecycleId { get; private init; }

    /// <summary>
    /// The name of the phase (e.g., "Planning", "Execution", "Closure").
    /// </summary>
    public string Name
    {
        get;
        private set => field = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    } = default!;

    /// <summary>
    /// A description of the phase's purpose and expected activities.
    /// </summary>
    public string Description
    {
        get;
        private set => field = Guard.Against.NullOrWhiteSpace(value, nameof(Description)).Trim();
    } = default!;

    /// <summary>
    /// The display order of the phase within the lifecycle.
    /// </summary>
    public int Order { get; internal set; }

    /// <summary>
    /// Updates the phase details.
    /// </summary>
    internal Result Update(string name, string description)
    {
        Name = name;
        Description = description;
        return Result.Success();
    }
}
