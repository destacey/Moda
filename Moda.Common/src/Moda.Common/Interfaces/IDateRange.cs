namespace Moda.Common.Interfaces;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TS">The type of the Start property.</typeparam>
/// <typeparam name="TE">The type of the End property.</typeparam>
public interface IDateRange<TS, TE>
{
    TS Start { get; }
    TE End { get; }

    bool Includes(TS value);
}


public interface IDateRange<T> : IDateRange<T, T>
{
}
