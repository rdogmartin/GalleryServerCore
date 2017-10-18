using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Controller;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GalleryServer.Web.Api
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // (AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)
    public class AlbumsController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly AlbumController _albumController;
        private readonly UserController _userController;
        private readonly ExceptionController _exController;

        public AlbumsController(AlbumController albumController, UserController userController, ExceptionController exController)
        {
            _albumController = albumController;
            _userController = userController;
            _exController = exController;
        }

        /// <summary>
        /// Gets the album with the specified <paramref name="id" />. The properties 
        /// <see cref="Entity.Album.GalleryItems" /> and <see cref="Entity.Album.MediaItems" /> 
        /// are set to null to keep the instance small. Example: api/albums/4/get
        /// </summary>
        /// <param name="id">The album ID.</param>
        /// <returns>An instance of <see cref="IActionResult" />.</returns>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        //[Authorize(Policy = GlobalConstants.PolicyViewAlbumOrAsset)]
        public async Task<IActionResult> Get(int id)
        {
            IAlbum album = null;
            try
            {
                album = _albumController.LoadAlbumInstance(new AlbumLoadOptions(id) { InflateChildObjects = true });

                // Must authorize authenticated users here. Can't do it as an attribute because we can't allow anonymous and execute the 
                // ViewAlbumOrAssetHandler for authenticated users. Maybe possible if we write our own attribute?
                //var userName = HttpContext.User.Claims.SingleOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.ViewAlbumOrMediaObject, await _userController.GetGalleryServerRolesForUser(), album.Id, album.GalleryId, _userController.IsAuthenticated, album.IsPrivate, album.IsVirtualAlbum);
                var permissionsEntity = await _albumController.GetPermissionsEntity(album);

                return new JsonResult(_albumController.ToAlbumEntity(album, permissionsEntity, new Entity.GalleryDataLoadOptions()));
            }
            catch (InvalidAlbumException)
            {
                return NotFound($"Could not find album with ID {id}.");
                //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                //{
                //    Content = new StringContent(String.Format("Could not find album with ID = {0}", id)),
                //    ReasonPhrase = "Album Not Found"
                //});
            }
            catch (GallerySecurityException ex)
            {
                AppEventController.LogError(ex);

                return Forbid();
                //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex, album?.GalleryId);

                return StatusCode(500, _exController.GetExString(ex));

                //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                //{
                //    Content = Utils.GetExStringContent(ex),
                //    ReasonPhrase = "Server Error"
                //});
            }
        }
    }
}
