using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using SteveTheTradeBot.Api.AppStartup;
using SteveTheTradeBot.Core.Components.Users;
using SteveTheTradeBot.Dal.Models.Auth;
using SteveTheTradeBot.Dal.Models.Users;
using Bumbershoot.Utilities.Helpers;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Validation;

namespace SteveTheTradeBot.Api.Security
{
    public class UserClaimProvider : IProfileService, IResourceOwnerPasswordValidator
    {
        private readonly IRoleManager _roleManager;
        private readonly IUserLookup _userLookup;


        public UserClaimProvider(IUserLookup userLookup, IRoleManager roleManager)
        {
            _userLookup = userLookup;
            _roleManager = roleManager;
        }

        #region IProfileService Members

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject.GetSubjectId();

            var user = await _userLookup.GetUserByEmail(sub);

            var claims = BuildClaimListForUser(user);

            context.IssuedClaims = claims;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userLookup.GetUserByEmail(sub);
            context.IsActive = user != null;
        }

        #endregion

        #region IResourceOwnerPasswordValidator Members

        #region Implementation of IResourceOwnerPasswordValidator

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var user = await _userLookup.GetUserByEmailAndPassword(context.UserName, context.Password);
            if (user != null)
            {
                var claims = BuildClaimListForUser(user);
                context.Result = new GrantValidationResult(
                    user.Id,
                    "password",
                    claims
                );
            }
        }

        #endregion

        #endregion

        public static string ToPolicyName(Activity claim)
        {
            return claim.ToString().ToLower();
        }

        #region Private Methods

        private List<Claim> BuildClaimListForUser(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Name, user.Email),
                new Claim(JwtClaimTypes.Id, user.Id),
                new Claim(JwtClaimTypes.GivenName, user.Name),
                new Claim(IdentityServerConstants.StandardScopes.Email, user.Email),
                new Claim(JwtClaimTypes.Scope, IocApi.Instance.Resolve<OpenIdSettings>().ScopeApi),
                user.Roles.Contains(RoleManager.Admin.Name)
                    ? new Claim(JwtClaimTypes.Role, RoleManager.Admin.Name)
                    : new Claim(JwtClaimTypes.Role, RoleManager.Guest.Name)
            };
            var selectMany = user.Roles.Select(r => _roleManager.GetRoleByName(r).Result).SelectMany(x => x.Activities)
                .Distinct().ToList();
            foreach (var claim in selectMany) claims.Add(new Claim(JwtClaimTypes.Role, ToPolicyName(claim)));

            return claims;
        }

        #endregion
    }
}