namespace Moda.Common.Extensions;

public static class GenericExtensions
{
    public static IEnumerable<T> FlattenHierarchy<T>(this T root, Func<T, IEnumerable<T>?> branchSelector)
    {
        ArgumentNullException.ThrowIfNull(branchSelector);

        Stack<T> stack = new();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            yield return current;

            if (current is null)
                continue;

            foreach (var child in branchSelector(current) ?? [])
            {
                stack.Push(child);
            }
        }
    }

    public static IEnumerable<TOut> FlattenHierarchy<TIn,TOut>(
        this TIn root, 
        Func<TIn, IEnumerable<TIn>?> branchSelector, 
        Func<TIn, TOut> projection)
    {
        ArgumentNullException.ThrowIfNull(branchSelector);

        Stack<TIn> stack = new();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            yield return projection(current);

            if (current is null)
                continue;

            foreach (var child in branchSelector(current) ?? [])
            {
                stack.Push(child);
            }
        }
    }

    public static IEnumerable<TOut> FlattenHierarchy<TIn, TOut>(
        this TIn root,
        Func<TIn, IEnumerable<TIn>?> branchSelector,
        Func<TIn, TOut> projection,
        Func<TOut, TOut> projectionEnricher)
    {
        ArgumentNullException.ThrowIfNull(branchSelector);

        Stack<TIn> stack = new();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            var mappedValue = projection(current);
            yield return projectionEnricher(mappedValue);

            if (current is null)
                continue;

            foreach (var child in branchSelector(current) ?? [])
            {
                stack.Push(child);
            }
        }
    }
}
