using SteveTheTradeBot.Dal.Models.Base;

namespace SteveTheTradeBot.Dal.Models.Users
{
    public class UserReference : BaseReferenceWithName
    {
        public string Email { get; set; }
    }
}