using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;

namespace Moda.Planning.Domain.Models.PlanningPoker;

public sealed class EstimationScale : BaseAuditableEntity<int>
{
    private readonly List<string> _values = [];

    private EstimationScale() { }

    private EstimationScale(string name, string? description)
    {
        Name = name;
        Description = description;
        IsActive = true;
    }

    /// <summary>
    /// The name of the Estimation Scale.
    /// </summary>
    public string Name
    {
        get;
        private set => field = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    } = default!;

    /// <summary>
    /// The description of the Estimation Scale.
    /// </summary>
    public string? Description
    {
        get;
        private set => field = value.NullIfWhiteSpacePlusTrim();
    }

    /// <summary>
    /// Whether the estimation scale is active and available for use.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// The ordered values in this estimation scale.
    /// </summary>
    public IReadOnlyList<string> Values => _values.AsReadOnly();

    /// <summary>
    /// Update the estimation scale name and description.
    /// </summary>
    public Result Update(string name, string? description)
    {
        try
        {
            Name = name;
            Description = description;
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    /// <summary>
    /// Activate the estimation scale.
    /// </summary>
    public Result Activate()
    {
        if (IsActive)
            return Result.Failure("The estimation scale is already active.");

        IsActive = true;
        return Result.Success();
    }

    /// <summary>
    /// Deactivate the estimation scale.
    /// </summary>
    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure("The estimation scale is already inactive.");

        IsActive = false;
        return Result.Success();
    }

    /// <summary>
    /// Replace the scale values with a new set.
    /// </summary>
    public Result SetValues(IEnumerable<string> values)
    {
        var valueList = values.ToList();
        if (valueList.Count < 2)
            return Result.Failure("An estimation scale must have at least 2 values.");

        try
        {
            _values.Clear();
            foreach (var value in valueList)
            {
                _values.Add(Guard.Against.NullOrWhiteSpace(value, nameof(value)).Trim());
            }
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    /// <summary>
    /// Create a new estimation scale.
    /// </summary>
    public static Result<EstimationScale> Create(string name, string? description, IEnumerable<string> values)
    {
        var valueList = values.ToList();
        if (valueList.Count < 2)
            return Result.Failure<EstimationScale>("An estimation scale must have at least 2 values.");

        try
        {
            var scale = new EstimationScale(name, description);

            foreach (var value in valueList)
            {
                scale._values.Add(Guard.Against.NullOrWhiteSpace(value, nameof(value)).Trim());
            }

            return Result.Success(scale);
        }
        catch (Exception ex)
        {
            return Result.Failure<EstimationScale>(ex.ToString());
        }
    }
}
