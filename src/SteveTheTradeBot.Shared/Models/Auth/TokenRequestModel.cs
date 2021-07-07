namespace SteveTheTradeBot.Shared.Models.Auth
{
    public class TokenRequestModel
    {
        public TokenRequestModel()
        {
            GrantType = "password";
        }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string ClientId { get; set; }

        public string GrantType { get; set; }

        public string ClientSecret { get; set; }
    }
}