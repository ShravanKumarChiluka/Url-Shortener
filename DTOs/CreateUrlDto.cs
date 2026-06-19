namespace UrlShortener.DTOs
{
    public class CreateUrlDto
    {
        public string OriginalUrl { get; set; } = string.Empty;
        public string? CustomAlias { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
