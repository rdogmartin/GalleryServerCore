using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace GalleryServer.Web.Security
{
    public class GalleryAdminHandler : AuthorizationHandler<AdminRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
        {
            context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
