namespace Moda.Common.Interfaces;
public interface IDateRange<T>
{
    T Start { get; }
    T End { get; }

    bool Includes(T value);
    bool Includes(IDateRange<T> range);
    bool Overlaps(IDateRange<T> range);
}
