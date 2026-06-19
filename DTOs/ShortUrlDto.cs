namespace UrlShortener.DTOs
{
    public class ShortUrlDto
    {
        public int Id { get; set; }
        public string OriginalUrl { get; set; } = string.Empty;
        public string ShortCode { get; set; } = string.Empty;
        public string ShortUrl {  get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } 
        public DateTime? ExpiresAt { get; set; }
        public int TotalClicks  { get; set; }
    }
}
