using Microsoft.AspNetCore.Http;

namespace SteveTheTradeBot.Api.WebApi.Controllers
{
    public static class WebApiHelper
    {
        public static string GetQuery(this HttpRequest request)
        {
            return request.Path;
        }

        public static string GetName(this IHttpContextAccessor httpContextAccessor)
        {
            return httpContextAccessor.HttpContext.User?.Identity?.Name;
        }
    }
}