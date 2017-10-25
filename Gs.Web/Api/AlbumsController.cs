using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Controller;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GalleryServer.Business.Metadata;
using GalleryServer.Web.Entity;
using GalleryServer.Web.Security;

namespace GalleryServer.Web.Api
{
    /// <summary>
    /// Contains methods for Web API access to albums.
    /// </summary>
    //[Route("api/[controller]/[action]")]
    //[AllowAnonymous]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // (AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)
    public class AlbumsController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly AlbumController _albumController;
        private readonly GalleryObjectController _galleryObjectController;
        private readonly UserController _userController;
        private readonly ExceptionController _exController;
        private readonly IAuthorizationService _authorizationService;

        public AlbumsController(AlbumController albumController, GalleryObjectController galleryObjectController, UserController userController, ExceptionController exController, IAuthorizationService authorizationService)
        {
            _albumController = albumController;
            _galleryObjectController = galleryObjectController;
            _userController = userController;
            _exController = exController;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Gets the album with the specified <paramref name="id" />. The properties 
        /// <see cref="Entity.Album.GalleryItems" /> and <see cref="Entity.Album.MediaItems" /> 
        /// are set to null to keep the instance small. Example: api/albums/4/get
        /// </summary>
        /// <param name="id">The album ID.</param>
        /// <returns>An instance of <see cref="IActionResult" />.</returns>
        [AllowAnonymous]
        [HttpGet] //("{id:int}")
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
        [AllowAnonymous]
        [HttpGet, ActionName("Inflated")]
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
        /// Gets the gallery items for the specified album, optionally sorting the results.
        /// </summary>
        /// <param name="id">The album ID.</param>
        /// <param name="sortByMetaNameId">The name of the metadata item to sort on.</param>
        /// <param name="sortAscending">If set to <c>true</c> sort in ascending order.</param>
        /// <returns>IQueryable{Entity.GalleryItem}.</returns>
        [AllowAnonymous]
        [HttpGet, ActionName("GalleryItems")]
        public async Task<IActionResult> GetGalleryItemsForAlbumId(int id, int sortByMetaNameId = int.MinValue, bool sortAscending = true)
        {
            // GET /api/albums/galleryitems/12?sortByMetaNameId=11&sortAscending=true - Gets gallery items for album #12
            try
            {
                return new JsonResult(await _galleryObjectController.GetGalleryItemsInAlbum(id, (MetadataItemName)sortByMetaNameId, sortAscending));
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
                AppEventController.LogError(ex);

                return StatusCode(500, _exController.GetExString(ex));
            }
        }

        /// <summary>
        /// Gets the media items for the specified album.
        /// </summary>
        /// <param name="id">The album ID.</param>
        /// <param name="sortByMetaNameId">The name of the metadata item to sort on.</param>
        /// <param name="sortAscending">If set to <c>true</c> sort in ascending order.</param>
        /// <returns>IQueryable{Entity.MediaItem}.</returns>
        [AllowAnonymous]
        [HttpGet, ActionName("MediaItems")]
        public async Task<IActionResult> GetMediaItemsForAlbumId(int id, int sortByMetaNameId = int.MinValue, bool sortAscending = true)
        {
            // GET /api/albums/mediaitems/12 - Gets media items for album #12
            try
            {
                return new JsonResult(await _galleryObjectController.GetMediaItemsInAlbum(id, (MetadataItemName) sortByMetaNameId, sortAscending));
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
                AppEventController.LogError(ex);

                return StatusCode(500, _exController.GetExString(ex));
            }
        }

        /// <summary>
        /// Gets the meta items for the specified album <paramref name="id" />.
        /// </summary>
        /// <param name="id">The album ID.</param>
        /// <returns>IQueryable&lt;Entity.MetaItem&gt;.</returns>
        /// <exception cref="StringContent"></exception>
        [AllowAnonymous]
        [HttpGet, ActionName("Meta")]
        public async Task<IActionResult> GetMetaItemsForAlbumId(int id)
        {
            // GET /api/albums/meta/12 - Gets metadata items for album #12
            try
            {
                return new JsonResult(await _albumController.GetMetaItemsForAlbum(id));
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
                AppEventController.LogError(ex);

                return StatusCode(500, _exController.GetExString(ex));
            }
        }

        /// <summary>
        /// Persists the <paramref name="album" /> to the data store. Only the following properties are persisted:
        /// <see cref="Entity.Album.SortById" />, <see cref="Entity.Album.SortUp" />, <see cref="Entity.Album.IsPrivate" />
        /// </summary>
        /// <param name="album">The album to persist.</param>
        /// <returns>Task&lt;IActionResult&gt;.</returns>
        /// <exception cref="ArgumentNullException">album</exception>
        [HttpPost]
        //[Authorize(Policy = GlobalConstants.PolicyOperationAuthorization)]
        public async Task<IActionResult> Post([FromBody]Entity.Album album)
        {
            // POST api/albums/post
            if (album == null)
                throw new ArgumentNullException(nameof(album));

            try
            {
                var alb = Factory.LoadAlbumInstance(new AlbumLoadOptions(album.Id) { IsWritable = true });

                if ((await _authorizationService.AuthorizeAsync(User, alb, Operations.EditAlbum)).Succeeded)
                {
                    await _albumController.UpdateAlbum(album);

                    return Ok($"Album {alb.Id} saved...");
                }
                else
                {
                    throw new GallerySecurityException($"You do not have permission '{Operations.EditAlbum.RequestedPermission}' for album ID {album.Id}.");
                }
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

        /// <summary>
        /// Create an album based on <paramref name="album" />. The only properties used in the <paramref name="album" /> parameter are
        /// <see cref="Entity.Album.Title" /> and <see cref="Entity.Album.ParentId" />. If <see cref="Entity.Album.GalleryId" /> is 
        /// specified and an error occurs, it is used to help with error logging. Other properties are ignored, but if they need to be
        /// persisted in the future, this method can be modified to persist them. The parent album is resorted after the album is added.
        /// </summary>
        /// <param name="album">An <see cref="Entity.Album" /> instance containing data to be persisted to the data store.</param>
        /// <returns>The ID of the newly created album.</returns>
        [HttpPut, ActionName("CreateAlbum")]
        public async Task<IActionResult> Put(Entity.Album album)
        {
            try
            {
                await _albumController.CreateAlbum(album);

                return new JsonResult(new Business.ActionResult
                {
                    Status = ActionResultStatus.Success.ToString(),
                    Title = $"Successfully created album {album.Title}",
                    Message = string.Empty,
                    ActionTarget = album
                });
            }
            catch (InvalidAlbumException ex)
            {
                return new JsonResult(new Business.ActionResult()
                {
                    Status = ActionResultStatus.Error.ToString(),
                    Title = "Cannot Create Album",
                    Message = ex.Message
                });
            }
            catch (GallerySecurityException ex)
            {
                AppEventController.LogError(ex);

                return Forbid();
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex, album.GalleryId);

                return StatusCode(500, _exController.GetExString(ex));
            }
        }

        /// <summary>
        /// Deletes the album with the specified <paramref name="id" /> from the data store.
        /// </summary>
        /// <param name="id">The ID of the album to delete.</param>
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _albumController.DeleteAlbum(id);

                return Ok($"Album {id} deleted...");
            }
            catch (InvalidAlbumException)
            {
                // HTTP specification says the DELETE method must be idempotent, so deleting a nonexistent item must have 
                // the same effect as deleting an existing one. So we simply return HttpStatusCode.OK.
                return Ok($"Album with ID {id} does not exist.");
            }
            catch (GallerySecurityException ex)
            {
                AppEventController.LogError(ex);

                return Forbid();
            }
            catch (CannotDeleteAlbumException ex)
            {
                AppEventController.LogError(ex);

                return Forbid();
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex);

                return StatusCode(500, _exController.GetExString(ex));
            }
        }


        /// <summary>
        /// Sorts the <paramref name="album" /> by the <see cref="Entity.Album.SortById" /> and <see cref="Entity.Album.SortUp" /> properties,
        /// optionally updating the album with this sort preference. When <paramref name="persistToAlbum" /> is <c>true</c>, a physical album
        /// must be specified (ID > 0). When <c>false</c> and the album is virtual, the <see cref="Entity.Album.GalleryItems" /> property must 
        /// be specified.
        /// </summary>
        /// <param name="album">The album to be sorted.</param>
        /// <param name="persistToAlbum">if set to <c>true</c> the album is updated to use the specified sort preferences for all users.</param>
        /// <returns>IQueryable&lt;Entity.GalleryItem&gt;.</returns>
        [HttpPost, ActionName("SortAlbum")]
        public async Task<IActionResult> Sort(Entity.Album album, bool persistToAlbum)
        {
            try
            {
                if (persistToAlbum)
                {
                    // Change the sort of an existing album for all users.
                    if (album.Id <= 0)
                    {
                        throw new ArgumentException("An album ID must be specified when calling the AlbumsController.Sort() Web.API method with the persistToAlbum parameter set to true.");
                    }

                    await _albumController.Sort(album.Id, album.SortById, album.SortUp);

                    return new JsonResult(_galleryObjectController.ToGalleryItems(_albumController.LoadAlbumInstance(album.Id).GetChildGalleryObjects().ToSortedList()));
                }
                else
                {
                    return new JsonResult(_galleryObjectController.SortGalleryItems(album));
                }
            }
            catch (InvalidAlbumException)
            {
                return NotFound($"Could not find album with ID {album.Id}. It may have been deleted by another user.");
            }
            catch (GallerySecurityException ex)
            {
                AppEventController.LogError(ex);

                return Forbid();
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex);

                return StatusCode(500, _exController.GetExString(ex));
            }
        }

        /// <summary>
        /// Moves the <paramref name="itemsToMove" /> to the <paramref name="destinationAlbumId" />.
        /// </summary>
        /// <param name="destinationAlbumId">The ID of the destination album.</param>
        /// <param name="itemsToMove">The items to transfer.</param>
        /// <returns>An instance of <see cref="IActionResult" />.</returns>
        [HttpPost, ActionName("MoveToAlbum")]
        public IActionResult MoveTo(int destinationAlbumId, Entity.GalleryItem[] itemsToMove)
        {
            // POST /api/albums/movetoalbum?destinationAlbumId=99
            return TransferTo(destinationAlbumId, itemsToMove, GalleryAssetTransferType.Move);
        }

        /// <summary>
        /// Copies the <paramref name="itemsToCopy" /> to the <paramref name="destinationAlbumId" />.
        /// </summary>
        /// <param name="destinationAlbumId">The ID of the destination album.</param>
        /// <param name="itemsToCopy">The items to transfer.</param>
        /// <returns>An instance of <see cref="IActionResult" />.</returns>
        [HttpPost, ActionName("CopyToAlbum")]
        public IActionResult CopyTo(int destinationAlbumId, Entity.GalleryItem[] itemsToCopy)
        {
            // POST /api/albums/copytoalbum?destinationAlbumId=99
            return TransferTo(destinationAlbumId, itemsToCopy, GalleryAssetTransferType.Copy);
        }

        /// <summary>
        /// Moves or copies the <paramref name="itemsToTransfer" /> to the <paramref name="destinationAlbumId" />.
        /// </summary>
        /// <param name="destinationAlbumId">The ID of the destination album.</param>
        /// <param name="itemsToTransfer">The items to transfer.</param>
        /// <param name="transferType">Type of the transfer.</param>
        /// <returns>An instance of <see cref="IActionResult" />.</returns>
        private IActionResult TransferTo(int destinationAlbumId, Entity.GalleryItem[] itemsToTransfer, GalleryAssetTransferType transferType)
        {
            try
            {
                //TODO: Need to implement TransferToAlbum()
                var destinationAlbum = _albumController.TransferToAlbum(destinationAlbumId, itemsToTransfer, transferType, out var createdGalleryItems);

                return new JsonResult(new Business.ActionResult()
                {
                    Status = ActionResultStatus.Success.ToString(),
                    Title = $"{transferType} Successful",
                    Message = "The items were transferred.",
                    ActionTarget = createdGalleryItems
                });
            }
            catch (GallerySecurityException)
            {
                return new JsonResult(new Business.ActionResult()
                {
                    Status = ActionResultStatus.Error.ToString(),
                    Title = "Transfer Aborted - Invalid Selection",
                    Message = "You do not have permission to move or copy media assets to the selected album or you selected an album in a read-only gallery. Review your selection."
                });
            }
            catch (InvalidAlbumException ex)
            {
                return new JsonResult(new Business.ActionResult()
                {
                    Status = ActionResultStatus.Error.ToString(),
                    Title = "Transfer Aborted - Invalid Selection",
                    Message = ex.Message
                });
            }
            catch (CannotTransferAlbumToNestedDirectoryException ex)
            {
                return new JsonResult(new Business.ActionResult()
                {
                    Status = ActionResultStatus.Error.ToString(),
                    Title = "Transfer Aborted - Invalid Selection",
                    Message = ex.Message
                });
            }
            catch (UnsupportedMediaObjectTypeException ex)
            {
                return new JsonResult(new Business.ActionResult()
                {
                    Status = ActionResultStatus.Error.ToString(),
                    Title = "Transfer Aborted - Disabled File Type",
                    Message = $"One or more media assets you selected is a disabled file type ({System.IO.Path.GetExtension(ex.MediaObjectFilePath)}). An administrator can enable this file type on the File Types page."
                });
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex);

                return StatusCode(500, _exController.GetExString(ex));
            }
        }
    }
}
