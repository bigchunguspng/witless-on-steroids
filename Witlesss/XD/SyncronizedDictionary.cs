using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Witlesss.XD
{
    public class SyncronizedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly object _sync = new();
        private Dictionary<TKey, TValue> _dictionary = new();

        public object Sync => _sync;

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            lock (_sync) return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        public void Add      (KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
        public bool Remove   (KeyValuePair<TKey, TValue> item) => Contains(item) && Remove(item.Key);
        public bool Contains (KeyValuePair<TKey, TValue> item)
        {
            lock (_sync) return _dictionary.Contains(item);
        }

        public bool Remove      (TKey key) { lock (_sync) return _dictionary.Remove(key); }
        public bool ContainsKey (TKey key) { lock (_sync) return _dictionary.ContainsKey(key); }

        public void Add    (TKey key, TValue value) { lock (_sync)        _dictionary.   Add(key, value); }
        public bool TryAdd (TKey key, TValue value) { lock (_sync) return _dictionary.TryAdd(key, value); }

        public bool TryGetValue (TKey key, out TValue value)
        {
            lock (_sync) return _dictionary.TryGetValue(key, out value);
        }

        public void Clear()
        {
            lock (_sync) _dictionary = new Dictionary<TKey, TValue>();
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => throw new NotImplementedException();

        public int  Count { get { lock (_sync) return _dictionary.Count; } }

        public bool IsReadOnly => false;

        public TValue this[TKey key]
        {
            get { lock (_sync) return _dictionary[key];         }
            set { lock (_sync)        _dictionary[key] = value; }
        }

        public ICollection<TKey>   Keys   { get { lock (_sync) return _dictionary.Keys;   } }
        public ICollection<TValue> Values { get { lock (_sync) return _dictionary.Values; } }
    }
}