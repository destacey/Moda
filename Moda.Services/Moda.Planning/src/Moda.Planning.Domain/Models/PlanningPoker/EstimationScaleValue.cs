using Ardalis.GuardClauses;

namespace Moda.Planning.Domain.Models.PlanningPoker;

public class EstimationScaleValue
{
    private EstimationScaleValue() { }

    internal EstimationScaleValue(string value, int order)
    {
        Value = Guard.Against.NullOrWhiteSpace(value, nameof(Value)).Trim();
        Order = Guard.Against.Negative(order, nameof(Order));
    }

    public int EstimationScaleId { get; private set; }

    public string Value { get; private set; } = default!;

    public int Order { get; private set; }
}
