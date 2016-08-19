using AspNetIdentity.WebApi.Infrastructure;
using AspNetIdentity.WebApi.Providers;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Serialization;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;

namespace AspNetIdentity.WebApi
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration httpConfig = new HttpConfiguration();

            ConfigureOAuthTokenGeneration(app);

            ConfigureOAuthTokenConsumption(app);

            ConfigureWebApi(httpConfig);

            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            app.UseWebApi(httpConfig);
        }

        private void ConfigureOAuthTokenGeneration(IAppBuilder app)
        {
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationRoleManager>(ApplicationRoleManager.Create);

            OAuthAuthorizationServerOptions OAuthServerOptions = new OAuthAuthorizationServerOptions() 
            { 
                AllowInsecureHttp = true,
                TokenEndpointPath = new Microsoft.Owin.PathString("/oauth/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(1),
                Provider = new CustomOAuthProvider(),
                AccessTokenFormat = new CustomJwtFormat("http://webapi.localhost")
            };

            app.UseOAuthAuthorizationServer(OAuthServerOptions);
        }

        private void ConfigureWebApi(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }

        private void ConfigureOAuthTokenConsumption(IAppBuilder app)
        {
            var issuer = "http://webapi.localhost";

            string audienceId = ConfigurationManager.AppSettings["as:AudienceId"];
            byte[] audienceSecret = TextEncodings.Base64Url.Decode(ConfigurationManager.AppSettings["as:AudienceSecret"]);

            app.UseJwtBearerAuthentication(
                new JwtBearerAuthenticationOptions
                {
                    AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Active,
                    AllowedAudiences = new[] { audienceId },
                    IssuerSecurityTokenProviders = new IIssuerSecurityTokenProvider[] 
                    { 
                        new SymmetricKeyIssuerSecurityTokenProvider(issuer, audienceSecret)
                    }
                }
                );
        }

    }
}