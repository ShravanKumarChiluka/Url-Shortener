namespace UrlShortener.Models
{
    public class Click
    {
        public int Id { get; set; }
        public int ShortUrlId { get; set; }
        public ShortUrl? ShortUrl { get; set; }
        public DateTime ClickedAt { get; set; } = DateTime.UtcNow;
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string Referrer { get; set; }
    }
}
