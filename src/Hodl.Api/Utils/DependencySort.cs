namespace Hodl.Api.Utils;

public static class DependencySort
{
    public static IList<T> Sort<T>(List<T> values, IEnumerable<(T, T)> dependencies)
    {
        values.Sort();
        values.Reverse();
        values.Sort((x, y) => dependencies.Any(i => i.Item1.Equals(x) && i.Item2.Equals(y)) ? 1 : -1);

        return values;
    }
}
