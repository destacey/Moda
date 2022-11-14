using Ardalis.GuardClauses;

namespace Moda.Common.Extensions;
public static class EnumerableExtensions
{
    public static IEnumerable<IList<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
    {
        Guard.Against.Null(source);
        Guard.Against.OutOfRange(batchSize, nameof(batchSize), 1, int.MaxValue);
        
        List<T> list = new(batchSize);
        foreach (T item in source)
        {
            list.Add(item);
            if (list.Count == batchSize)
            {
                yield return list;
                list = new List<T>(batchSize);
            }
        }

        if (list.Count > 0)
        {
            yield return list;
        }
    }
}
