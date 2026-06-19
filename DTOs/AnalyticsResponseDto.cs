namespace UrlShortener.DTOs
{
    public class AnalyticsResponseDto
    {
        public int TotalClicks { get; set; }
        public IEnumerable<ClicksByDayDto> ClicksByDay { get; set; } = [];
        public IEnumerable<TopReferrerDto> TopReferrer { get; set; } = [];
        public IEnumerable<TopBrowserDto> TopBrowser { get; set; } = [];

    }
    public class ClicksByDayDto
    {
        public DateOnly Date {  get; set; }
        public int Count { get; set; }
    }
    public class TopReferrerDto
    {
        public string Referrer { get; set; } = string.Empty;
        public int Count { get; set; }
    }
    public class TopBrowserDto
    {
        public string UserAgent { get; set; } = string.Empty;
        public int Count { get; set; }
    }

}
