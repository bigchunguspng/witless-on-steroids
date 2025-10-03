namespace PF_Tools.Backrooms.Types.Collections;

/// Queue of a limited capacity.
public class LimitedQueue<T> : Queue<T>
{
    private readonly int _limit;

    public LimitedQueue(int limit) : base(limit)
    {
        _limit = limit;
    }

    public LimitedQueue(int limit, IEnumerable<T> collection) : base(collection)
    {
        _limit = limit;

        if (Count > limit) throw new ArgumentException("Query count exceds the limit!", nameof(collection));
    }

    /// If the limit is reached,
    /// the 1st element is dequeued and returned.
    public new T? Enqueue(T item)
    {
        var removed = Count < _limit
            ? default
            : Dequeue();

        base.Enqueue(item);

        return removed;
    }
}