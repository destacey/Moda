using Ardalis.GuardClauses;

namespace Moda.Common.Extensions;
public static class EnumerableExtensions
{
    /// <summary>Batches the specified batch size.</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">The source.</param>
    /// <param name="batchSize">Size of the batch.</param>
    /// <returns></returns>
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

    /// <summary>Nots the null and any.</summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source">The source.</param>
    /// <returns></returns>
    public static bool NotNullAndAny<T>(this IEnumerable<T>? source)
    {
        return source is not null && source.Any();
    }
}
