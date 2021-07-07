using System.Collections.Generic;
using SteveTheTradeBot.Dal.Models.Auth;

namespace SteveTheTradeBot.Dal.Models.Users
{
    public class Role
    {
        public string Name { get; set; }
        public List<Activity> Activities { get; set; }
    }
}