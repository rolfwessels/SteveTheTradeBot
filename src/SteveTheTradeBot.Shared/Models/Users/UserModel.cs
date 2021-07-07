using System;
using System.Collections.Generic;
using SteveTheTradeBot.Shared.Models.Shared;

namespace SteveTheTradeBot.Shared.Models.Users
{
    public class UserModel : BaseModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Image { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public List<string> Roles { get; set; }

        public List<string> Activities { get; set; }

        public override string ToString()
        {
            return $"Id: {Id}, Email: {Email}, Name: {Name}";
        }
    }
}