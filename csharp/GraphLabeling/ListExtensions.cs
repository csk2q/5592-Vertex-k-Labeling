namespace GraphLabeling;

public static class ListExtensions
{
    public static void InsertSorted<T>(this List<T> list, T value, IComparer<T>? comparer = null)
        where T : IComparable<T>
    {
        if (list == null) throw new ArgumentNullException(nameof(list));

        comparer ??= Comparer<T>.Default;
        int index = list.BinarySearch(value, comparer);
        if (index < 0) index = ~index;
        list.Insert(index, value);
    }
}
