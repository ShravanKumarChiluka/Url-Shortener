using Microsoft.EntityFrameworkCore;
using UrlShortener.Data;
using UrlShortener.DTOs;
using UrlShortener.Models;

namespace UrlShortener.Services
{
    public class UrlService
    {
        private readonly AppDbContext _db;
        private readonly CacheService _cache;
        private readonly ShortCodeService _shortcode;
        private readonly IConfiguration _config;

        public UrlService(AppDbContext db, CacheService cache, ShortCodeService shortcode, IConfiguration config)
        {
            _db = db;
            _cache = cache;
            _shortcode = shortcode;
            _config = config;
        }

        public async Task<ShortUrlDto> CreateAsync(CreateUrlDto dto, int UserId)
        {
            string code;
            if (!string.IsNullOrWhiteSpace(dto.CustomAlias))
            {
                if (await _db.ShortUrls.AnyAsync(u => u.CustomAlias == dto.CustomAlias))
                    throw new InvalidOperationException("Custom alias already taken");
                code = dto.CustomAlias;
            }
            else
            {
                do { code = _shortcode.Generate(); }
                while( await _db.ShortUrls.AnyAsync(u => u.ShortCode == code));
            }

            var shortUrl = new ShortUrl
            {
                OriginalUrl = dto.OriginalUrl,
                ShortCode = code,
                CustomAlias = dto.CustomAlias,
                UserId = UserId,
                ExpiresAt = dto.ExpiresAt
            };

            _db.ShortUrls.Add(shortUrl);
            await _db.SaveChangesAsync();
            await _cache.SetAsync(code, shortUrl.OriginalUrl, TimeSpan.FromHours(24));

            return MapToDto(shortUrl);
        }

        public async Task<IEnumerable<ShortUrlDto>> GetUserUrlsAsync(int UserId)
        {
            var urls = await _db.ShortUrls
                .Include(u => u.Clicks)
                .Where(u => u.UserId == UserId)
                .ToListAsync();
            return urls.Select(MapToDto);
        }

        public async Task<bool> DeleteAsync(int id, int UserId)
        {
            var url = await _db.ShortUrls.FirstOrDefaultAsync(u => u.Id == id && u.UserId == UserId);
            if (url == null) return false;

            _db.ShortUrls.Remove(url);
            await _db.SaveChangesAsync();
            await _cache.RemoveAsync(url.ShortCode);
            return true;
        }

        public async Task<string?> ResolveAsync(string code)
        {
            var cached = await _cache.GetAsync(code);
            if (cached != null) return cached;

            var url = await _db.ShortUrls.FirstOrDefaultAsync(u => u.ShortCode == code || u.CustomAlias == code);

            if (url == null) return null;
            if (url.ExpiresAt.HasValue && url.ExpiresAt < DateTime.UtcNow) return null;

            await _cache.SetAsync(code, url.OriginalUrl, TimeSpan.FromHours(24));
            return url.OriginalUrl;
        }

        public async Task RecordClickAsync(string code, HttpRequest request)
        {
            var url = await _db.ShortUrls.FirstOrDefaultAsync(u => u.ShortCode == code || u.CustomAlias == code);
            if (url == null) return;

            _db.Clicks.Add(new Click
            {
                ShortUrlId = url.Id,
                IpAddress = request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
                UserAgent = request.Headers.UserAgent.ToString(),
                Referrer = request.Headers.Referer.ToString()
            });

            await _db.SaveChangesAsync();
        }

        public async Task<AnalyticsResponseDto?> GetAnalyticsAsync(int id, int userid)
        {
            var url = await _db.ShortUrls
                .Include(u => u.Clicks)
                .FirstOrDefaultAsync(u => u.Id == id && u.UserId == userid);

            if (url == null) return null;

            return new AnalyticsResponseDto
            {
                TotalClicks = url.Clicks.Count,
                ClicksByDay = url.Clicks
                .GroupBy(c=>DateOnly.FromDateTime(c.ClickedAt))
                .Select(g=> new ClicksByDayDto { Date = g.Key, Count = g.Count()})
                .OrderBy(x=>x.Date),
                TopReferrer = url.Clicks
                      .GroupBy(c => c.Referrer)
                      .Select(g => new TopReferrerDto { Referrer = g.Key, Count = g.Count() })
                      .OrderByDescending(x => x.Count)
                      .Take(5),
                TopBrowser = url.Clicks
                      .GroupBy(c => c.UserAgent)
                      .Select(g => new TopBrowserDto { UserAgent = g.Key, Count = g.Count() })
                      .OrderByDescending(x => x.Count)
                      .Take(5)
            };
        }

        private ShortUrlDto MapToDto(ShortUrl url) => new()
        {
            Id = url.Id,
            OriginalUrl = url.OriginalUrl,
            ShortCode = url.ShortCode,
            ShortUrl = $"{_config["BaseUrl"]}/{url.ShortCode}",
            CreatedAt = url.CreatedAt,
            ExpiresAt = url.ExpiresAt,
            TotalClicks = url.Clicks.Count
        };
    }
}
