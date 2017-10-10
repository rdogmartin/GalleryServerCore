//using GalleryServer.Web.Controller;
//using Microsoft.AspNetCore.Authorization;
//using System.Linq;
//using System.Security.Claims;
//using System.Threading.Tasks;

//namespace GalleryServer.Web.Security
//{
//    public class ViewAlbumOrAssetHandler : AuthorizationHandler<ViewAlbumOrAssetRequirement>
//    {
//        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ViewAlbumOrAssetRequirement requirement)
//        {
//            var roleNames = context.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
//            var roles = await RoleController.GetGalleryServerRoles(roleNames);

//            if (roles.Any(role => role.AllAlbumIds.Contains(1)))
//            {
//                context.Succeed(requirement);
//            }

//            //return Task.CompletedTask;
//        }
//    }
//}
