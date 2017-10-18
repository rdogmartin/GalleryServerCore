using GalleryServer.Web.Controller;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GalleryServer.Web.Security
{
    public class SiteAdminHandler : AuthorizationHandler<AdminRequirement>
    {
        private readonly RoleController _roleController;

        // DI for UserManager doesn't work here. Get this error:
        // InvalidOperationException: Cannot consume scoped service &#x27;Microsoft.AspNetCore.Identity.UserManager`1[GalleryServer.Data.GalleryUser]&#x27; from singleton &#x27;Microsoft.AspNetCore.Authorization.IAuthorizationHandler&#x27;.
        public SiteAdminHandler(RoleController roleController)
        {
            _roleController = roleController;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
        {
            var roleNames = context.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
            var roles = await _roleController.GetGalleryServerRoles(roleNames);

            if (roles.Any(role => role.AllowAdministerSite))
            {
                context.Succeed(requirement);
            }

            //return Task.CompletedTask;
        }
    }
}
