using System;
using SteveTheTradeBot.Core.Vendor;
using SteveTheTradeBot.Dal.Models.Users;
using Bumbershoot.Utilities.Helpers;

namespace SteveTheTradeBot.Core.Components.Users
{
    public static class UserDalHelper
    {
        public static bool IsPassword(this User user, string password)
        {
            return PasswordHash.ValidatePassword(password, user.HashedPassword);
        }

        public static string SetPassword(this User user, string password)
        {
            return user.HashedPassword = SetPassword(password);
        }

        public static string SetPassword(string password)
        {
            return PasswordHash.CreateHash(password ?? Guid.NewGuid().ToString());
        }

        public static void ValidateRolesAndThrow(this User user)
        {
            if (!RoleManager.AreValidRoles(user.Roles))
                throw new ArgumentException($"One or more role does not exist [{user.Roles.StringJoin()}]");
        }
    }
}