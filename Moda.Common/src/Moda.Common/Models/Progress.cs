using CSharpFunctionalExtensions;

namespace Moda.Common.Models;

public class Progress : ValueObject
{
    public Progress(decimal value)
    {
        if (value < 0.0m || value > 100.0m)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "Progress must be between 0.0 and 100.0");
        }

        Value = value;
    }

    public decimal Value { get; }

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Value;
    }

    public static Progress NotStarted() => new(0.0m);
    public static Progress Completed() => new(100.0m);

    public override string ToString() => $"{Value}%";

    public static implicit operator decimal(Progress progress) => progress.Value;
    public static explicit operator Progress(decimal value) => new(value);
}
