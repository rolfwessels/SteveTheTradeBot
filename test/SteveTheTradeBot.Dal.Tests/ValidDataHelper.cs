using System.Text.RegularExpressions;
using SteveTheTradeBot.Core.Framework.Mappers;
using SteveTheTradeBot.Dal.Models.Projects;
using SteveTheTradeBot.Dal.Models.Users;
using FizzWare.NBuilder;
using FizzWare.NBuilder.Generators;

namespace SteveTheTradeBot.Dal.Tests
{
    public static class ValidDataHelper
    {
        public static ISingleObjectBuilder<T> WithValidData<T>(this ISingleObjectBuilder<T> value)
        {
            return value.With(ValidData);
        }

        public static IListBuilder<T> WithValidData<T>(this IListBuilder<T> value)
        {
            return value.All().With(ValidData);
        }

        #region Private Methods

        private static T ValidData<T>(T value)
        {
            if (value is Project project)
                project.Name = GetRandom.String(20);

            if (value is User user)
            {
                user.Name = GetRandom.FirstName() + " " + GetRandom.LastName();
                user.Email = (Regex.Replace(user.Name.ToLower(), "[^a-z]", "") + GetRandom.NumericString(3) +
                              "@nomailmail.com").ToLower();
                user.HashedPassword = GetRandom.String(20);
                user.Roles.Add("Guest");
            }

            var userGrant = value as UserGrant;
            if (userGrant != null) userGrant.User = Builder<User>.CreateNew().Build().ToReference();
            return value;
        }

        #endregion
    }
}