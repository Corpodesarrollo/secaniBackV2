using Microsoft.Extensions.Caching.Memory;

namespace Core.Services.MSPermisos
{
    public class CacheService
    {
        private readonly IMemoryCache _memoryCache;
        private MemoryCacheEntryOptions cacheEntryOptions;

        public CacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            cacheEntryOptions =
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(120),
                    SlidingExpiration = TimeSpan.FromMinutes(30)
                };
        }

        public void CacheData(string key, object data)
        {
            _memoryCache.Set(key, data);
        }

        public object GetData(string key)
        {
            _memoryCache.TryGetValue(key, out var data);
            return data;
        }
    }
}
