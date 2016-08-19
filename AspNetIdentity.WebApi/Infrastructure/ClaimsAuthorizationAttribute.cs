using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Filters;
using System.Web.Http;
using System.Net.Http;

namespace AspNetIdentity.WebApi.Infrastructure
{
    public class ClaimsAuthorizationAttribute : AuthorizationFilterAttribute
    {
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }

        public override Task OnAuthorizationAsync(System.Web.Http.Controllers.HttpActionContext actionContext, System.Threading.CancellationToken cancellationToken)
        {
            //return base.OnAuthorizationAsync(actionContext, cancellationToken);
            var principal = actionContext.RequestContext.Principal as ClaimsPrincipal;
            if(!principal.Identity.IsAuthenticated)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                return Task.FromResult<object>(null);
            }

            if(!(principal.HasClaim(x => x.Type == ClaimType && x.Value == ClaimValue)))
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                return Task.FromResult<object>(null);
            }

            return Task.FromResult<object>(null);
        }
    }
}