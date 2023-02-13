using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Witlesss.X
{
    public class SyncronizedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly object _sync;
        private Dictionary<TKey, TValue> _dictionary;
        
        public SyncronizedDictionary()
        {
            _dictionary = new Dictionary<TKey, TValue>();
            _sync = new object();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            lock (_sync) return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            lock (_sync) _dictionary.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            lock (_sync) _dictionary = new Dictionary<TKey, TValue>();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            lock (_sync) return _dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            var x = Contains(item);
            if (x) Remove(item.Key);
            return x;
        }

        public int Count
        {
            get
            {
                lock (_sync) return _dictionary.Count;
            }
        }

        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            lock (_sync) _dictionary.Add(key, value);
        }

        public bool TryAdd(TKey key, TValue value)
        {
            var x = !ContainsKey(key);

            if (x) lock (_sync) Add(key, value);

            return x;
        }

        public bool ContainsKey(TKey key)
        {
            lock (_sync) return _dictionary.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            lock (_sync) return _dictionary.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var x = ContainsKey(key);
            value = x ? this[key] : default;
            return x;
        }

        public TValue this[TKey key]
        {
            get
            {
                lock (_sync) return _dictionary[key];
            }
            set
            {
                lock (_sync) _dictionary[key] = value;
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                lock (_sync) return _dictionary.Keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                lock (_sync) return _dictionary.Values;
            }
        }
    }
}