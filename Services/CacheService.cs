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
    }
}
