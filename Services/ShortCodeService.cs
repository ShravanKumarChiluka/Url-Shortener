namespace UrlShortener.Services
{
    public class ShortCodeService
    {
        public const string Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private readonly Random _random = new();

        public string Generate(int Length = 7)
        {
            return new string(Enumerable.Range(0, Length).Select(_ => Chars[_random.Next(Chars.Length)]).ToArray());
        }
    }
}
