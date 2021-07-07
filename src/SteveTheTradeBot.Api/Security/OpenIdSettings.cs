using System.Linq;
using Bumbershoot.Utilities;
using Microsoft.Extensions.Configuration;

namespace SteveTheTradeBot.Api.Security
{
    public class OpenIdSettings : BaseSettings
    {
        

        public OpenIdSettings(IConfiguration configuration) : base(configuration, "OpenId")
        {
        }

        public string HostUrl => ReadConfigValue("HostUrl", "http://localhost:5000");

        public string ApiResourceName => ReadConfigValue("ApiResourceName", "api.resource");

        public string ApiResourceSecret => ReadConfigValue("ApiResourceSecret", "a98802aa-e4d4-432a-835e-6c856a05d999");

        public string ClientName => ReadConfigValue("ClientName", "stevethetradebot.api");

        public string ClientSecret => ReadConfigValue("ClientSecret", "super_secure_password");

        public string IdentPath => ReadConfigValue("IdentPath", "identity");

        public string ScopeApi => ReadConfigValue("ScopeApi", "api");

        public string Origins => ReadConfigValue("Origins", "http://localhost:5000");

        public string CertPfx => ReadConfigValue("CertPfx", "development.pfx");

        public string CertPassword => ReadConfigValue("CertPassword", "60053018f4794862a82982640570c552");

        public string CertStoreThumbprint => ReadConfigValue("CertStoreThumbprint", "");
        //B75303B3E5CEBE484C342D438987AB33560B5717

        public bool UseReferenceTokens => ReadConfigValue("UseReferenceTokens", false);

        public bool IsDebugEnabled => ReadConfigValue("IsDebugEnabled", true);

        public string[] GetOriginList()
        {
            return Origins.Split(',').ToArray();
        }
    }
}