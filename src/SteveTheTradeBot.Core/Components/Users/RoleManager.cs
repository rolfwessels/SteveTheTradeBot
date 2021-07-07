using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SteveTheTradeBot.Dal.Models.Auth;
using SteveTheTradeBot.Dal.Models.Users;
using Bumbershoot.Utilities.Helpers;

namespace SteveTheTradeBot.Core.Components.Users
{
    public class RoleManager : IRoleManager
    {
        private static readonly List<Role> _roles;
        public static Role Admin = new Role {Name = "Admin", Activities = EnumHelper.ToArray<Activity>().ToList()};

        public static Role Guest = new Role
        {
            Name = "Guest",
            Activities = EnumHelper.ToArray<Activity>()
                .Where(x => x != Activity.ReadUsers && (x.ToString().StartsWith("Read") || x == Activity.Subscribe))
                .ToList()
        };

        static RoleManager()
        {
            _roles = new List<Role>
            {
                Admin,
                Guest
            };
        }

        public static List<Role> All => _roles;

        #region IRoleManager Members

        public Task<Role> GetRoleByName(string name)
        {
            return Task.FromResult(GetRole(name));
        }

        public Task<List<Role>> Get()
        {
            return Task.FromResult(_roles.ToList());
        }

        #endregion

        public static Role GetRole(string name)
        {
            return _roles.FirstOrDefault(x => x.Name == name);
        }

        public static bool IsAuthorizedActivity(Activity[] activities, params string[] roleName)
        {
            if (roleName.Contains(Admin.Name)) return true;
            var allActivities = Activities(roleName).ToArray();
            return activities.All(allActivities.Contains);
        }

        #region Private Methods

        private static IEnumerable<Activity> Activities(IEnumerable<string> rolesByName)
        {
            return _roles.Where(x => rolesByName.Contains(x.Name)).SelectMany(x => x.Activities).ToArray();
        }

        #endregion

        public static bool AreValidRoles(List<string> userRoles)
        {
            var roles = _roles.Select(x => x.Name).ToArray();
            return userRoles.All(role => roles.Contains(role));
        }

        public static string[] GetRoles(Activity permission)
        {
            return _roles.Where(x => x.Activities.Contains(permission)).Select(x => x.Name).ToArray();
        }
    }
}