using System;
using System.Collections.Generic;

namespace Syp.Extensions
{
    public static class CacheSupport
    {
        private static TV OrCache<TK, TV>(Func<TK, TV> fnGet, IDictionary<TK, TV> cache, TK key)
        {
            if (cache.ContainsKey(key))
            {
                return cache[key];
            }

            var data = fnGet(key);
            cache.Add(key, data);
            return data;
        }


        public static TV GetOrCache<TK, TV>(this TK key, Func<TK, TV> fnGet, IDictionary<TK, TV> cache)
        {
            lock (cache)
            {
                return OrCache(fnGet, cache, key);
            }
        }
    }
}