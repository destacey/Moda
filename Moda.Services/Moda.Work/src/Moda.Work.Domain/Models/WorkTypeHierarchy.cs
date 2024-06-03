using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Domain.Enums.Work;
using NodaTime;

namespace Moda.Work.Domain.Models;

/// <summary>
/// This object ensures the integrity and system state of global work type levels.
/// </summary>
public class WorkTypeHierarchy : BaseEntity<int>, ISystemAuditable
{
    private readonly List<WorkTypeLevel> _levels = [];

    private WorkTypeHierarchy() { }

    private WorkTypeHierarchy(Instant timestamp)
    {
        _levels.AddRange(GetSystemDefaultWorkTypeLevels(timestamp));
    }

    /// <summary>
    /// List of child work type levels.
    /// </summary>
    public IReadOnlyList<WorkTypeLevel> Levels => _levels.AsReadOnly();

    /// <summary>
    /// Add a work type level to the scheme.  The work type level category must be a Portfolio Tier.
    /// </summary>
    /// <param name="workTypeLevel"></param>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public Result<WorkTypeLevel> AddPortfolioWorkTypeLevel(string name, string? description, Instant timestamp)
    {
        Guard.Against.NullOrWhiteSpace(name);

        if (_levels.Any(l => l.Name == name.Trim()))
            return Result.Failure<WorkTypeLevel>($"A work type level with the name '{name}' already exists.");

        var maxOrder = _levels.Where(l => l.Tier == WorkTypeTier.Portfolio).Max(l => l.Order);

        var level = WorkTypeLevel.Create(name, description, WorkTypeTier.Portfolio, Ownership.Owned, maxOrder + 1, timestamp);

        _levels.Add(level);
        AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));
        return Result.Success(level);
    }

    /// <summary>
    /// Update an existing work type level.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public Result UpdateWorkTypeLevel(int id,
            string name,
            string? description,
            Instant timestamp)
    {
        var workTypeLevel = _levels.FirstOrDefault(x => x.Id == id);
        if (workTypeLevel is null)
            return Result.Failure<int>("Work Type Level not found.");

        if (_levels.Where(l => l.Id != id).Any(l => l.Name == name.Trim()))
            return Result.Failure($"A work type level with the name '{name}' already exists.");

        var result = workTypeLevel.Update(name, description, timestamp);
        if (result.IsFailure)
            return Result.Failure<int>(result.Error);

        AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));
        return Result.Success();
    }

    public Result Reinitialize(Instant timestamp)
    {
        try
        {
            bool hasChanged = false;
            List<WorkTypeLevel> defaultWorkTypeLevels = GetSystemDefaultWorkTypeLevels(timestamp);

            foreach (var defaultWorkTypeLevel in defaultWorkTypeLevels)
            {
                var existing = _levels.FirstOrDefault(x => x.Name == defaultWorkTypeLevel.Name && x.Ownership == Ownership.System);
                if (existing is not null)
                {
                    // Update existing work type level if it has changed
                    if (existing.Description != defaultWorkTypeLevel.Description
                        || existing.Order != defaultWorkTypeLevel.Order)
                    {
                        var result = existing.Update(
                            defaultWorkTypeLevel.Name,
                            defaultWorkTypeLevel.Description,
                            timestamp);

                        if (result.IsSuccess)
                            hasChanged = true;
                    }
                }
                else
                {
                    // Add new work type level since it does not exist
                    _levels.Add(defaultWorkTypeLevel);
                    hasChanged = true;
                }
            }

            if (hasChanged)
                AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    /// <summary>Initializes the global work type hierarchy.  This should only ever called once.</summary>
    /// <param name="timestamp">The timestamp.</param>
    /// <returns></returns>
    public static WorkTypeHierarchy Initialize(Instant timestamp)
    {
        WorkTypeHierarchy scheme = new(timestamp);

        scheme.AddDomainEvent(EntityCreatedEvent.WithEntity(scheme, timestamp));
        return scheme;
    }

    private static List<WorkTypeLevel> GetSystemDefaultWorkTypeLevels(Instant timestamp)
    {
        // work type levels cannot be removed or have their name changed in this list without changes to the current ReInitialize process
        List<WorkTypeLevel> levels =
        [
            WorkTypeLevel.Create("Stories", "Stories work type level.", WorkTypeTier.Requirement, Ownership.System, 1, timestamp),

            WorkTypeLevel.Create("Tasks", "Tasks work type level.", WorkTypeTier.Task, Ownership.System, 1, timestamp),

            WorkTypeLevel.Create("Other", "Work type level for work types not mapped to one of the primary work type levels.", WorkTypeTier.Other, Ownership.System, 1, timestamp),

            WorkTypeLevel.Create("Epics", "Epics work type level.", WorkTypeTier.Portfolio, Ownership.System, 1, timestamp),
            WorkTypeLevel.Create("Features", "Features work type level.", WorkTypeTier.Portfolio, Ownership.System, 2, timestamp)
        ];

        return levels;
    }
}
