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
    //[Route("api/[controller]/[action]")]
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
        [HttpGet] //("{id:int}")
        [AllowAnonymous]
        //[Authorize(Policy = GlobalConstants.PolicyViewAlbumOrAsset)]
        public async Task<IActionResult> Get(int id)
        {
            // GET /api/albums/get/12 // Return data for album # 12
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

        /// <summary>
        /// Gets a comprehensive set of data about the specified album.
        /// </summary>
        /// <param name="id">The album ID.</param>
        /// <param name="top">Specifies the number of child gallery objects to retrieve. Specify 0 to retrieve all items.</param>
        /// <param name="skip">Specifies the number of child gallery objects to skip.</param>
        /// <returns>An instance of <see cref="Entity.GalleryData" />.</returns>
        [HttpGet,ActionName("Inflated")]
        [AllowAnonymous]
        public async Task<IActionResult> GetInflatedAlbum(int id, int top = 0, int skip = 0)
        {
            // GET /api/albums/inflated/12 // Return data for album # 12
            IAlbum album = null;
            try
            {
                album = Factory.LoadAlbumInstance(new AlbumLoadOptions(id) { InflateChildObjects = true });
                var loadOptions = new Entity.GalleryDataLoadOptions
                {
                    LoadGalleryItems = true,
                    NumGalleryItemsToRetrieve = top,
                    NumGalleryItemsToSkip = skip
                };

                SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.ViewAlbumOrMediaObject, await _userController.GetGalleryServerRolesForUser(), album.Id, album.GalleryId, _userController.IsAuthenticated, album.IsPrivate, album.IsVirtualAlbum);

                return new JsonResult(await _albumController.GetGalleryDataForAlbum(album, loadOptions));
            }
            catch (InvalidAlbumException)
            {
                return NotFound($"Could not find album with ID {id}.");
            }
            catch (GallerySecurityException ex)
            {
                AppEventController.LogError(ex);

                return Forbid();
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex, album?.GalleryId);

                return StatusCode(500, _exController.GetExString(ex));
            }
        }

        /// <summary>
        /// Persists the <paramref name="album" /> to the data store. Only the following properties are persisted: 
        /// <see cref="Entity.Album.SortById" />, <see cref="Entity.Album.SortUp" />, <see cref="Entity.Album.IsPrivate" />
        /// </summary>
        /// <param name="album">The album to persist.</param>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Entity.Album album)
        {
            // POST api/albums/post
            try
            {
                await _albumController.UpdateAlbum(album);

                return Ok("Album saved...");
            }
            catch (InvalidAlbumException)
            {
                return NotFound($"Could not find album with ID {album.Id}.");
            }
            catch (GallerySecurityException ex)
            {
                AppEventController.LogError(ex);

                return Forbid();
            }
            catch (NotSupportedException ex)
            {
                return StatusCode(500, $"Business Rule Violation: {_exController.GetExString(ex)}");
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex, album?.GalleryId);

                return StatusCode(500, _exController.GetExString(ex));
            }
        }
    }
}
