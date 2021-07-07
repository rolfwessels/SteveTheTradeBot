using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using RestSharp;

namespace SteveTheTradeBot.Sdk.Helpers
{
    public static class RestSharpHelper
    {
        public static int MaxLogLength { get; set; } = 400;
        public static Action<string> Log { get; set; } = message => { };

        public static async Task<IRestResponse<T>> ExecuteAsyncWithLogging<T>(this RestClient client,
            RestRequest request) where T : new()
        {
            var method = request.Method;
            var buildUri = client.BuildUri(request);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var paramsSent = request.Parameters.Where(x => x.Name == "application/json").Select(x => x.Value.ToString())
                .FirstOrDefault();

            Log($"Sent {method} {buildUri} [{Truncate(paramsSent, MaxLogLength)}]");

            var response = await client.ExecuteAsync<T>(request);
            stopwatch.Stop();
            Log($"Response {method} {buildUri} [{stopwatch.ElapsedMilliseconds}ms] [{Truncate(response.Content, MaxLogLength)}]");

            return response;
        }


        public static string Truncate(string value, int maxChars)
        {
            if (value == null) return null;
            return value.Length <= maxChars ? value : value.Substring(0, maxChars) + "...";
        }
    }
}