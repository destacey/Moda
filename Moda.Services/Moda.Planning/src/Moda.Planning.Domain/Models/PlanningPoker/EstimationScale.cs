using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Domain.Interfaces;

namespace Moda.Planning.Domain.Models.PlanningPoker;

public class EstimationScale : BaseEntity<int>, ISystemAuditable
{
    private readonly List<EstimationScaleValue> _values = [];

    private EstimationScale() { }

    private EstimationScale(string name, string? description, bool isPreset)
    {
        Name = name;
        Description = description;
        IsPreset = isPreset;
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
    /// Indicates whether this is a built-in preset scale.
    /// </summary>
    public bool IsPreset { get; private set; }

    /// <summary>
    /// The ordered values in this estimation scale.
    /// </summary>
    public IReadOnlyCollection<EstimationScaleValue> Values => _values.OrderBy(v => v.Order).ToList().AsReadOnly();

    /// <summary>
    /// Update the estimation scale name and description.
    /// </summary>
    public Result Update(string name, string? description)
    {
        if (IsPreset)
            return Result.Failure("Preset estimation scales cannot be modified.");

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
    /// Replace the scale values with a new set.
    /// </summary>
    public Result SetValues(IEnumerable<(string Value, int Order)> values)
    {
        if (IsPreset)
            return Result.Failure("Preset estimation scale values cannot be modified.");

        var valueList = values.ToList();
        if (valueList.Count < 2)
            return Result.Failure("An estimation scale must have at least 2 values.");

        if (valueList.Select(v => v.Order).Distinct().Count() != valueList.Count)
            return Result.Failure("Estimation scale values must have unique order values.");

        try
        {
            _values.Clear();
            foreach (var (value, order) in valueList)
            {
                _values.Add(new EstimationScaleValue(value, order));
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
    public static Result<EstimationScale> Create(string name, string? description, bool isPreset, IEnumerable<(string Value, int Order)> values)
    {
        var valueList = values.ToList();
        if (valueList.Count < 2)
            return Result.Failure<EstimationScale>("An estimation scale must have at least 2 values.");

        if (valueList.Select(v => v.Order).Distinct().Count() != valueList.Count)
            return Result.Failure<EstimationScale>("Estimation scale values must have unique order values.");

        try
        {
            var scale = new EstimationScale(name, description, isPreset);

            foreach (var (value, order) in valueList)
            {
                scale._values.Add(new EstimationScaleValue(value, order));
            }

            return Result.Success(scale);
        }
        catch (Exception ex)
        {
            return Result.Failure<EstimationScale>(ex.ToString());
        }
    }

    /// <summary>
    /// Create a new preset estimation scale. Used for seeding.
    /// </summary>
    public static Result<EstimationScale> CreatePreset(string name, string? description, IEnumerable<(string Value, int Order)> values)
    {
        return Create(name, description, isPreset: true, values);
    }
}
