using AspNetIdentity.WebApi.Infrastructure;
using AspNetIdentity.WebApi.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AspNetIdentity.WebApi.Controllers
{
    [RoutePrefix("api/account")]
    public class AccountsController : BaseApiController
    {
        [Authorize(Roles="Admin")]
        [Route("users")]
        public IHttpActionResult GetUsers()
        {
            return Ok(this.AppUserManager.Users.ToList().Select(u => this.TheModelFactory.Create(u)));
        }

        [Authorize(Roles="Admin")]
        [HttpGet]
        [Route("user/{id:guid}", Name="GetUserById")]
        public async Task<IHttpActionResult> GetUser(string Id)
        {
            var user = await this.AppUserManager.FindByIdAsync(Id);

            if(user != null)
            {
                return Ok(this.TheModelFactory.Create(user));
            }

            return NotFound();
        }

        [Authorize(Roles="Admin")]
        [Route("user/{username}")]
        public async Task<IHttpActionResult> GetUserByName(string username)
        {
            var user = await this.AppUserManager.FindByNameAsync(username);

            if(user != null)
            {
                return Ok(this.TheModelFactory.Create(user));
            }

            return NotFound();
        }

        [AllowAnonymous]
        [Route("create")]
        public async Task<IHttpActionResult> CreateUser(CreateUserBindingModel createUserModel)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser()
            {
                UserName = createUserModel.UserName,
                Email = createUserModel.Email,
                FirstName = createUserModel.FirstName,
                LastName = createUserModel.LastName,
                Level = 3,
                JoinDate = DateTime.Now.Date
            };

            IdentityResult addUserResult = await this.AppUserManager.CreateAsync(user, createUserModel.Password);

            if(!addUserResult.Succeeded)
            {
                return GetErrorResult(addUserResult);
            }

            //#sending emails 
            //string code = await this.AppUserManager.GenerateEmailConfirmationTokenAsync(user.Id);
            //var callbackUrl = new Uri(Url.Link("ConfirmEmailRoute", new { userId = user.Id, code = code}));
            //await this.AppUserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
            //#end

            Uri locationHeader = new Uri(Url.Link("GetUserById", new { id = user.Id }));

            return Created(locationHeader, TheModelFactory.Create(user));
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ConfirmEmail", Name="ConfirmEmailRoute")]
        public async Task<IHttpActionResult> ConfirmEmail(string userId = "", string code = "")
        {
            if(string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
            {
                ModelState.AddModelError("", "User Id and Code are required");
                return BadRequest(ModelState);
            }

            IdentityResult result = await this.AppUserManager.ConfirmEmailAsync(userId, code);

            if(result.Succeeded)
            {
                return Ok();
            }
            else
            {
                return GetErrorResult(result);
            }
        }

        [Authorize(Roles="Admin")]
        [HttpDelete]
        [Route("user/{id:guid}")]
        public async Task<IHttpActionResult> DeleteUser(string id)
        {
            var appUser = await this.AppUserManager.FindByIdAsync(id);
            if(appUser != null)
            {
                IdentityResult result = await this.AppUserManager.DeleteAsync(appUser);
                if(!result.Succeeded)
                {
                    return GetErrorResult(result);
                }

                return Ok();
            }

            return NotFound();
        }

        [Authorize(Roles="Admin")]
        [Route("user/{id:guid}/roles")]
        [HttpPut]
        public async Task<IHttpActionResult> AssignRolesToUser([FromUri] string id, [FromBody] string[] rolesToAssign)
        {
            var appUser = await this.AppUserManager.FindByIdAsync(id);

            if(appUser == null)
            {
                return NotFound();
            }

            var currentRoles = await this.AppUserManager.GetRolesAsync(appUser.Id);
            var rolesNotExists = rolesToAssign.Except(this.AppRoleManager.Roles.Select(x => x.Name)).ToArray();

            if(rolesNotExists.Count() > 0)
            {
                ModelState.AddModelError("", string.Format("Roles '{0}' does not exixts in the system", string.Join(",", rolesNotExists)));
                return BadRequest(ModelState);
            }

            IdentityResult removeResult = await this.AppUserManager.RemoveFromRolesAsync(appUser.Id, currentRoles.ToArray());
            if(!removeResult.Succeeded)
            {
                return BadRequest(ModelState);
            }

            IdentityResult addResult = await this.AppUserManager.AddToRolesAsync(appUser.Id, rolesToAssign);
            if(!addResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to add user roles.");
                return BadRequest(ModelState);
            }

            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [Route("user/{id:guid}/assignclaims")]
        [HttpPut]
        public async Task<IHttpActionResult> AssignClaimsToUser([FromUri] string id, [FromBody] List<ClaimBindingModel> claimsToAssign)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var appUser = await this.AppUserManager.FindByIdAsync(id);
            if(appUser == null)
            {
                return NotFound();
            }

            foreach(ClaimBindingModel claimModel in claimsToAssign)
            {
                if(appUser.Claims.Any(c => c.ClaimType == claimModel.Type))
                {
                    await this.AppUserManager.RemoveClaimAsync(id, ExtendedClaimsProvider.CreateClaim(claimModel.Type, claimModel.Value));
                }

                await this.AppUserManager.AddClaimAsync(id, ExtendedClaimsProvider.CreateClaim(claimModel.Type, claimModel.Value));
            }

            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [Route("user/{id:guid}/removeclaims")]
        [HttpPut]
        public async Task<IHttpActionResult> RemoveClaimsFromUser([FromUri]string id, [FromBody]List<ClaimBindingModel> claimsToRemove)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var appUser = await this.AppUserManager.FindByIdAsync(id);

            if(appUser == null)
            {
                return NotFound();
            }

            foreach(ClaimBindingModel claimModel in claimsToRemove)
            {
                if(appUser.Claims.Any(c => c.ClaimType == claimModel.Type))
                {
                    await this.AppUserManager.RemoveClaimAsync(id, ExtendedClaimsProvider.CreateClaim(claimModel.Type, claimModel.Value));
                }
            }

            return Ok();
        }

        // POST api/Account/Register
        [AllowAnonymous]
        [Route("register")]
        public async Task<IHttpActionResult> Register(UserModel userModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser()
            {
                UserName = userModel.UserName,
                Email = Guid.NewGuid().ToString().Substring(0, 5) + "@qq.com",
                FirstName = Guid.NewGuid().ToString().Substring(0, 5),
                LastName = Guid.NewGuid().ToString().Substring(0, 5),
                Level = 3,
                JoinDate = DateTime.Now.Date,
                EmailConfirmed = true
            };

            IdentityResult addUserResult = await this.AppUserManager.CreateAsync(user, userModel.Password);

            if (!addUserResult.Succeeded)
            {
                return GetErrorResult(addUserResult);
            }

            Uri locationHeader = new Uri(Url.Link("GetUserById", new { id = user.Id }));

            return Created(locationHeader, TheModelFactory.Create(user));
        }

        [Authorize]
        [HttpGet]
        [Route("profile/{username}")]
        public async Task<IHttpActionResult> GetProfile(string username)
        {
            var user = await this.AppUserManager.FindByNameAsync(username);
            if (user != null)
            {
                return Ok(this.TheModelFactory.Create(user));
            }

            return NotFound();
        }
    }
}