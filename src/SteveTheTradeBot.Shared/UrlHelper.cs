namespace SteveTheTradeBot.Shared
{
    public static class UrlHelper
    {
        public static string SetParam(this string baseUrl, string param, string value)
        {
            return baseUrl.Replace($"{{{param}}}", value);
        }

        public static string AppendUrl(this string baseUrl, string appendToUrl)
        {
            if (!string.IsNullOrEmpty(appendToUrl)) return baseUrl + "/" + appendToUrl;
            return baseUrl;
        }
    }
}