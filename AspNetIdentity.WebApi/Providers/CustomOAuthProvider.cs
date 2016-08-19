using AspNetIdentity.WebApi.Infrastructure;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using System.Security.Claims;

namespace AspNetIdentity.WebApi.Providers
{
    public class CustomOAuthProvider : OAuthAuthorizationServerProvider
    {
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            //return base.ValidateClientAuthentication(context);
            context.Validated();
            return Task.FromResult<object>(null);
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            //return base.GrantResourceOwnerCredentials(context);
            var allowOrigin = "*";
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new []{allowOrigin});

            var userManager = context.OwinContext.GetUserManager<ApplicationUserManager>();

            ApplicationUser user = await userManager.FindAsync(context.UserName, context.Password);

            if(user == null)
            {
                context.SetError("invalid_grant", "The user name or password is incorrect");
                return;
            }

            if(!user.EmailConfirmed)
            {
                context.SetError("invalid_grant", "User did not confirm email.");
                return;
            }

            ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(userManager, "JWT");
            oAuthIdentity.AddClaims(ExtendedClaimsProvider.GetClaims(user));
            oAuthIdentity.AddClaims(RolesFromClaims.CreateRolesBasedOnClaims(oAuthIdentity));
            var ticket = new AuthenticationTicket(oAuthIdentity, null);
            context.Validated(ticket);
        }
    }
}