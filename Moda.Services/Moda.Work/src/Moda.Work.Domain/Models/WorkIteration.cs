using Moda.Common.Domain.Enums.Planning;
using Moda.Common.Domain.Interfaces.Planning.Iterations;
using Moda.Common.Domain.Models.Planning.Iterations;

namespace Moda.Work.Domain.Models;

/// <summary>
/// A copy of the Moda.Common.Domain.Interfaces.Planning.Iterations.ISimpleIteration interface.  Used to hold basic iteration information for the work service and db context.
/// </summary>
public sealed class WorkIteration : ISimpleIteration, IHasIdAndKey
{
    private WorkIteration() { }

    public WorkIteration(ISimpleIteration iteration)
    {
        Id = iteration.Id;
        Key = iteration.Key;
        Name = iteration.Name;
        Type = iteration.Type;
        State = iteration.State;
        DateRange = iteration.DateRange;
        TeamId = iteration.TeamId;
    }

    public Guid Id { get; private init; }
    public int Key { get; private init; }
    public string Name { get; private set; } = default!;
    public IterationType Type { get; private set; }
    public IterationState State { get; private set; }
    public IterationDateRange DateRange { get; private set; } = default!;
    public Guid? TeamId { get; private set; }

    /// <summary>
    /// Updates the current iteration with the values from the specified iteration.
    /// </summary>
    /// <param name="iteration">The iteration containing the updated values. The <see cref="ISimpleIteration.Id"/> must match the current
    /// iteration's ID.</param>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="iteration"/> ID does not match the current iteration's ID.</exception>
    public void Update(ISimpleIteration iteration)
    {
        if (iteration.Id != Id)
        {
            throw new ArgumentException("Iteration ID does not match.", nameof(iteration));
        }

        Name = iteration.Name;
        Type = iteration.Type;
        State = iteration.State;
        DateRange = iteration.DateRange;
        TeamId = iteration.TeamId;
    }
}
