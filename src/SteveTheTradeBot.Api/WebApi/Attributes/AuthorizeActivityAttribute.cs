using System.Reflection;
using SteveTheTradeBot.Api.Security;
using SteveTheTradeBot.Dal.Models.Auth;
using Serilog;
using Microsoft.AspNetCore.Authorization;

namespace SteveTheTradeBot.Api.WebApi.Attributes
{
    public class AuthorizeActivityAttribute : AuthorizeAttribute
    {
        public AuthorizeActivityAttribute()
        {
        }

        public AuthorizeActivityAttribute(Activity activities) : base(UserClaimProvider.ToPolicyName(activities))
        {
            Activities = activities;
        }

        public Activity Activities { get; }


        #region Overrides of AuthorizeAttribute

//		protected override bool IsAuthorized(HttpActionContext actionContext)
//		{
//			var isAuthorized = base.IsAuthorized(actionContext);
//			if (isAuthorized)
//			{
//				var identity = actionContext.RequestContext.Principal.Identity as ClaimsIdentity;
//				if (identity == null)
//				{
//					_log.Error("User not authorized because we were expecting a ClaimsIdentity");
//					return false;
//				}
//			    var roleName = identity.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToArray();
//			    isAuthorized = RoleManager.IsAuthorizedActivity(Activities, roleName);
//			}
//			return isAuthorized;
//		}

        #endregion
    }
}