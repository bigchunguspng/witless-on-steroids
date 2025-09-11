using System.Diagnostics.CodeAnalysis;

namespace PF_Tools.Backrooms.Types.Collections;

/// Cache of a limited size.
/// When it becomes full and a new item is added, it replaces the oldest one.
public class LimitedCache<TKey, TValue> where TKey : notnull
{
    private readonly int _limit;

    private readonly Queue<TKey> _keys;
    private readonly Dictionary<TKey, TValue> _paths;

    public LimitedCache(int limit)
    {
        _limit = limit;
        _keys = new Queue<TKey>(_limit);
        _paths = new Dictionary<TKey, TValue>(_limit);
    }

    public int Count => _keys.Count;

    public void Add(TKey id, TValue value)
    {
        if (_keys.Count == _limit)
        {
            var key = _keys.Dequeue();
            _paths.Remove(key);
        }
        _keys.Enqueue(id);
        _paths.TryAdd(id, value);
    }

    public bool Contains
        (TKey id, [MaybeNullWhen(false)] out TValue value)
    {
        return _paths.TryGetValue(id, out value);
    }

    public bool Contains_No
        (TKey id, [MaybeNullWhen(true)] out TValue value)
    {
        return Contains(id, out value).Janai();
    }
}