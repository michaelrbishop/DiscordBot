using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BishHouse2.Util
{
    public class InternalDictionary<T>
    {
        private ConcurrentDictionary<ulong, T> _dictionary;
        public InternalDictionary()
        {
            _dictionary = new ConcurrentDictionary<ulong, T>();
        }

        public void Add(ulong key, T value)
        {
            _dictionary.TryAdd(key, value);
        }

        public bool TryGetValue(ulong key, out T? value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Removes entries that match the given predicate.
        /// </summary>
        /// <param name="predicate"></param>
        public void RemoveBy(Func<T, bool> predicate)
        {
            var keysToRemove = _dictionary.Where(kvp => predicate(kvp.Value)).Select(kvp => kvp.Key).ToList();
            foreach (var key in keysToRemove)
            {
                _dictionary.TryRemove(key, out _);
            }
        }

        /// <summary>
        /// Clears the entire dictionary at once
        /// </summary>
        public void Dump()
        {
            _dictionary.Clear();
        }
    }
}
