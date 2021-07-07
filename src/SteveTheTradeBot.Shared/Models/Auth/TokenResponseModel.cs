namespace SteveTheTradeBot.Shared.Models.Auth
{
    public class TokenResponseModel
    {
        public string AccessToken { get; set; }

        public string TokenType { get; set; }

        public int ExpiresIn { get; set; }

        public string ClientId { get; set; }

        public string UserName { get; set; }

        public string DisplayName { get; set; }

        public string Permissions { get; set; }
    }
}