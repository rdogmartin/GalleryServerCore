using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Web.Controller;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace GalleryServer.Web.Security
{
    /// <summary>
    /// Implements an <see cref="AuthorizationHandler{TRequirement,TResource}" /> handler for album authorization. Uses
    /// <a href="https://docs.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased">Resource Based Authorization</a>.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Authorization.AuthorizationHandler{OperationAuthorizationRequirement, IAlbum}" />
    /// <see href="https://docs.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased" />
    /// <example>
    /// Because this handler requires an album, it cannot be used in an attribute. Instead, call it from within a method like this:
    /// <code>
    /// if ((await _authorizationService.AuthorizeAsync(User, album, Operations.EditAlbum)).Succeeded)
    /// {
    ///    // Perform task...
    /// }
    /// </code>
    /// </example>
    public class AlbumAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, IAlbum>
    {
        private readonly UserController _userController;

        public AlbumAuthorizationHandler(UserController userController)
        {
            _userController = userController;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, IAlbum album)
        {
            //var roleNames = context.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
            var roles = await _userController.GetGalleryServerRolesForUser();

            if (SecurityManager.IsUserAuthorized(requirement.RequestedPermission, roles, album.Id, album.GalleryId, _userController.IsAuthenticated, album.IsPrivate, album.IsVirtualAlbum))
            {
                context.Succeed(requirement);
            }
        }
    }
}
