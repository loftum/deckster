namespace Deckster.Core.Collections;

public static class StackExtensions
{
    public static void PushRange<T>(this Stack<T> set, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            set.Push(item);
        }
    }
}