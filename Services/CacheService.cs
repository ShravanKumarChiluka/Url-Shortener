using StackExchange.Redis;

namespace UrlShortener.Services
{
    public class CacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private static readonly TimeSpan DefaultExpiry = TimeSpan.FromHours(24);

        public CacheService(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<string> GetAsync(string key)
        {
            try
            {
                var value = await _redis.GetDatabase().StringGetAsync(key);
                return value.HasValue ? value.ToString() : null;
            }
            catch { return null; }
        }

        public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
        {
            try
            {
                await _redis.GetDatabase().StringSetAsync(key, value, expiry ?? DefaultExpiry);
            }
            catch { }
        }
        public async Task RemoveAsync(string key)
        {
            try
            {
                await _redis.GetDatabase().KeyDeleteAsync(key);
            }
            catch { }
        }
    }
}
