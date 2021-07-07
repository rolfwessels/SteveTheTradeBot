using Bumbershoot.Utilities;
using Microsoft.Extensions.Configuration;

namespace SteveTheTradeBot.Api
{
    public class ApiSettings : BaseSettings
    {
        public ApiSettings(IConfiguration configuration) : base(configuration, "Api")
        {
        }

        public string Origins => ReadConfigValue("Origins", "http://localhost:3000");
    }
}