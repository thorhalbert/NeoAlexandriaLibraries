using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BakedFileService.Utilities
{
    public class VolumeOpenCache : IVolumeCache
    {
        private IMemoryCache _cache;
        public VolumeOpenCache(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void Add<T>(T o, string key)
        {
            T cacheEntry = o;

            if (!_cache.TryGetValue(key, out cacheEntry))
            {
                cacheEntry = o;

                var entryOpts = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(7200));

                _cache.Set(key, cacheEntry, entryOpts);
            }
        }

        public T Get<T>(string key)
        {
            T cacheEntry;

            if (_cache.TryGetValue(key, out cacheEntry))
            {
                var entryOpts = new MemoryCacheEntryOptions()
                   .SetSlidingExpiration(TimeSpan.FromSeconds(7200));

                _cache.Set(key, cacheEntry, entryOpts);

                return cacheEntry;
            }

            return default(T);
        }

        public void Remove(string key)
        {
            _cache.Remove(key);
        }
    }
}
