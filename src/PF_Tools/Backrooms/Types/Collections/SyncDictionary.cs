using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace PF_Tools.Backrooms.Types.Collections;

/// Thread safe dictionary. Everything is locked on instance.
/// Unlike <see cref="System.Collections.Concurrent.ConcurrentDictionary&lt;TKey,TValue&gt;"/>,
/// this one <b>preserves</b> the elements order.
public class SyncDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : notnull
{
    private Dictionary<TKey, TValue> _dic = new();

    public bool IsReadOnly => false;

    public TValue this[TKey key]
    {
        get { lock (this) return _dic[key];         }
        set { lock (this)        _dic[key] = value; }
    }

    public int                 Count  { get { lock (this) return _dic.Count;  } }
    public ICollection<TKey>   Keys   { get { lock (this) return _dic.Keys;   } }
    public ICollection<TValue> Values { get { lock (this) return _dic.Values; } }

    [MethodImpl(Synchronized)] public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dic.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Add    (KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
    public bool Remove (KeyValuePair<TKey, TValue> item) => Contains(item) && Remove(item.Key);

    [MethodImpl(Synchronized)] public bool Contains    (KeyValuePair<TKey, TValue> item) => _dic.Contains(item);
    [MethodImpl(Synchronized)] public bool ContainsKey (TKey key) => _dic.ContainsKey(key);
    [MethodImpl(Synchronized)] public bool Remove      (TKey key) => _dic.Remove     (key);
    [MethodImpl(Synchronized)] public void Add         (TKey key, TValue value) => _dic.   Add(key, value);
    [MethodImpl(Synchronized)] public bool TryAdd      (TKey key, TValue value) => _dic.TryAdd(key, value);
    [MethodImpl(Synchronized)] public bool TryGetValue (TKey key, [MaybeNullWhen(false)] out TValue value) => _dic.TryGetValue(key, out value);
    [MethodImpl(Synchronized)] public void Clear       () => _dic = new Dictionary<TKey, TValue>();

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => throw new NotImplementedException();

    // LOOPS / ACTIONS

    [MethodImpl(Synchronized)] public T    Lock<T> (Func<Dictionary<TKey, TValue>, T>  func) => func  (_dic);
    [MethodImpl(Synchronized)] public void Lock    (Action<Dictionary<TKey, TValue>> action) => action(_dic);
    [MethodImpl(Synchronized)] public void ForEachPair    (Action<KeyValuePair<TKey, TValue>> action) => this.ForEach(action);
    [MethodImpl(Synchronized)] public void ForEachKey     (Action<TKey>   action) => Keys  .ForEach(action);
    [MethodImpl(Synchronized)] public void ForEachValue   (Action<TValue> action) => Values.ForEach(action);
}