namespace Moda.Common.Extensions;

public static class GenericExtensions
{
    public static IEnumerable<T> FlattenHierarchy<T>(this T root, Func<T, IEnumerable<T>> branchSelector)
    {
        ArgumentNullException.ThrowIfNull(branchSelector);

        Stack<T> stack = new ();
        stack.Push(root);
        
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            yield return current;

            if (current is null)
                continue;

            foreach (var child in branchSelector(current) ?? Enumerable.Empty<T>())
            {
                stack.Push(child);
            }
        }
    }
}
