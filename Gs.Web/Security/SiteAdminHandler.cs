using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using GalleryServer.Data;
using GalleryServer.Web.Controller;
using Microsoft.AspNetCore.Identity;

namespace GalleryServer.Web.Security
{
    public class SiteAdminHandler : AuthorizationHandler<AdminRequirement>
    {
        //private readonly UserManager<GalleryUser> _userManager;

        // DI for UserManager doesn't work here. Get this error:
        // InvalidOperationException: Cannot consume scoped service &#x27;Microsoft.AspNetCore.Identity.UserManager`1[GalleryServer.Data.GalleryUser]&#x27; from singleton &#x27;Microsoft.AspNetCore.Authorization.IAuthorizationHandler&#x27;.
        //public SiteAdminHandler(UserManager<GalleryUser> userManager)
        //{
        //    _userManager = userManager;
        //}

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
        {
            var roleNames = context.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
            var roles = await RoleController.GetGalleryServerRoles(roleNames);

            if (roles.Any(role => role.AllowAdministerSite))
            {
                context.Succeed(requirement);
            }

            //return Task.CompletedTask;
        }
    }
}
