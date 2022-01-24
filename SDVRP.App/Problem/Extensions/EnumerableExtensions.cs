namespace SDVRP.App.Problem.Extensions;

internal static class EnumerableExtensions
{
    public static bool NotContains<T>(this IEnumerable<T> enumerable, T item) => !enumerable.Contains(item);
}
