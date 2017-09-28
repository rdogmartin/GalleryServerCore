using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using GalleryServer.Web.Controller;

namespace GalleryServer.Web.Security
{
    public class SiteAdminHandler : AuthorizationHandler<AdminRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
        {
            var roleNames = context.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
            var roles = RoleController.GetGalleryServerRoles(roleNames);

            if (roles.Any(role => role.AllowAdministerSite))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
