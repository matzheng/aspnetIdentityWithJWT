using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace ResourceServer.Api
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();

            config.MapHttpAttributeRoutes();

            ConfigureOAuth(app);

            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);

            app.UseWebApi(config);
        }

        public void ConfigureOAuth(IAppBuilder app)
        {
            //var issuer = "http://jwtauthzsrv.azurewebsites.net";
            var issuer = "http://oauth.localhost";
            var audience = "8eb4b7fe63bc4090925243b06ecec235";
            var secret = TextEncodings.Base64Url.Decode("U64UgvsdPNDID1_-Ml95C2L6fwJr8rSAwrBdA1fyYUA");

            app.UseJwtBearerAuthentication(
            new JwtBearerAuthenticationOptions { 
                AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Active,
                AllowedAudiences = new[] { audience},
                IssuerSecurityTokenProviders = new IIssuerSecurityTokenProvider[] 
                { 
                    new SymmetricKeyIssuerSecurityTokenProvider(issuer, secret)
                },
                Provider = new OAuthBearerAuthenticationProvider
                {
                    OnValidateIdentity = context =>
                        {
                            context.Ticket.Identity.AddClaim(new System.Security.Claims.Claim("newCustomClaim", "newValue"));
                            return Task.FromResult<object>(null);
                        }
                }
            });
        }
    }
}