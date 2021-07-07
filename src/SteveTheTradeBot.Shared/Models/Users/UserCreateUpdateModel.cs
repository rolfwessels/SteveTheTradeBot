using System.Collections.Generic;

namespace SteveTheTradeBot.Shared.Models.Users
{
    public class UserCreateUpdateModel : RegisterModel
    {
        public List<string> Roles { get; set; }
    }
}