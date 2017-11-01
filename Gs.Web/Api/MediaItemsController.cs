using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Controller;
using GalleryServer.Web.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace GalleryServer.Web.Api
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MediaItemsController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly GalleryController _galleryController;
        private readonly AlbumController _albumController;
        private readonly GalleryObjectController _galleryObjectController;
        private readonly FileStreamController _streamController;
        private readonly HtmlController _htmlController;
        private readonly UrlController _urlController;
        private readonly UserController _userController;
        private readonly ExceptionController _exController;
        private readonly IAuthorizationService _authorizationService;

        public MediaItemsController(GalleryController galleryController, AlbumController albumController, GalleryObjectController galleryObjectController, FileStreamController streamController, HtmlController htmlController, UrlController urlController, UserController userController, ExceptionController exController, IAuthorizationService authorizationService)
        {
            _galleryController = galleryController;
            _albumController = albumController;
            _galleryObjectController = galleryObjectController;
            _streamController = streamController;
            _htmlController = htmlController;
            _urlController = urlController;
            _userController = userController;
            _exController = exController;
            _authorizationService = authorizationService;
        }

        /// <summary>
        /// Gets the media object with the specified <paramref name="id" />.
        /// </summary>
        /// <param name="id">The media object ID.</param>
        /// <returns>An instance of <see cref="Entity.MediaItem" />.</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get(int id)
        {
            // GET /api/mediaitems/get/4
            try
            {
                var settings = new AddMediaObjectSettings {AlbumId = 2, CurrentUserName = "Admin", DiscardOriginalFile = false, FileName = "Koala.jpg", FileNameOnServer = "Koala.jpg" };

                var results = await _galleryObjectController.AddMediaObject(settings);

                IGalleryObject mediaObject = Factory.LoadMediaObjectInstance(id);
                SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.ViewAlbumOrMediaObject, await _userController.GetGalleryServerRolesForUser(), mediaObject.Parent.Id, mediaObject.GalleryId, _userController.IsAuthenticated, mediaObject.Parent.IsPrivate, ((IAlbum)mediaObject.Parent).IsVirtualAlbum);
                var siblings = mediaObject.Parent.GetChildGalleryObjects(GalleryObjectType.MediaObject, !_userController.IsAuthenticated).ToSortedList();
                int mediaObjectIndex = siblings.IndexOf(mediaObject);

                return new JsonResult(_galleryObjectController.ToMediaItem(mediaObject, mediaObjectIndex + 1, _htmlController.GetMediaObjectHtmlBuilderOptions(mediaObject)));
            }
            catch (InvalidMediaObjectException)
            {
                return NotFound($"Could not find media object with ID {id}");
            }
            catch (GallerySecurityException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex);

                return StatusCode(500, _exController.GetExString(ex));
            }
        }

        /// <summary>
        /// Generate a ZIP archive for the requested <paramref name="galleryItems" /> and <paramref name="mediaSize" /> and persist to
        /// a file in the App_Data\_Temp directory. The filename is returned on the <see cref="Business.ActionResult.ActionTarget" /> property.
        /// </summary>
        /// <param name="galleryItems">The gallery items to download.</param>
        /// <param name="mediaSize">Size of the items to include in the ZIP archive.</param>
        /// <returns>An instance of <see cref="Business.ActionResult" />.</returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> PrepareZipDownload(Entity.GalleryItem[] galleryItems, DisplayObjectType mediaSize)
        {
            try
            {
                return new JsonResult(await _albumController.PrepareZipDownload(galleryItems, mediaSize));
            }
            catch (GallerySecurityException ex)
            {
                AppEventController.LogError(ex);

                return new JsonResult(new Business.ActionResult
                {
                    Title = "Cannot Download",
                    Status = ActionResultStatus.Warning.ToString(),
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex);

                return StatusCode(500, _exController.GetExString(ex));
            }
        }

        /// <summary>
        /// Retrieve the ZIP file having the name <paramref name="filename" />. It is expected that it was written to the App_Data\_Temp directory
        /// by a client call to <see cref="PrepareZipDownload" />. If it no longer exists (the app may have restarted, which clears out this directory),
        /// a 404 is returned to the client. No security check is performed since that was already done by <see cref="PrepareZipDownload" /> and it 
        /// would be highly unlikely for a user to guess the name of an existing file.
        /// </summary>
        /// <param name="filename">The filename. Example: "a5f23059-479e-456f-adac-a90d790d09b3.zip"</param>
        /// <returns>An instance of <see cref="System.Net.Http.HttpResponseMessage" />.</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult DownloadZip(string filename)
        {
            try
            {
                var stream = new FileStream(Path.Combine(AppSetting.Instance.TempUploadDirectory, filename), FileMode.Open);

                var mimeType = Factory.LoadMimeType("dummy.zip")?.FullType ?? "application/octet-stream";
                var zipFilename = _urlController.UrlEncode("Media Files".Replace(" ", "_") + ".zip");

                return File(stream, mimeType, zipFilename);
            }
            catch (FileNotFoundException)
            {
                return NotFound("ZIP Archive Not Found: The ZIP file was not found on the server. This can happen when temporary files are removed during an application recycle. Try again.");
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex);

                return StatusCode(500, _exController.GetExString(ex));
            }
        }

        /// <summary>
        /// Return the file for the requested media asset.
        /// </summary>
        /// <returns>IActionResult.</returns>
        [HttpGet, ActionName("File")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMediaFile(int id, DisplayObjectType dt)
        {
            // GET api/mediaitems/file/12?dt=2 // Get the optimized file for media # 12
            _streamController.SetMedia(id, dt);

            return File(await _streamController.GetStream(), _streamController.ContentType);
        }

        /// <summary>
        /// Gets a comprehensive set of data about the specified media object.
        /// </summary>
        /// <param name="id">The media object ID.</param>
        /// <returns>An instance of <see cref="Entity.GalleryData" />.</returns>
        [HttpGet, ActionName("Inflated")]
        [AllowAnonymous]
        public IActionResult GetInflatedMediaObject(int id)
        {
            try
            {
                var mediaObject = Factory.LoadMediaObjectInstance(id);

                return new JsonResult(_galleryController.GetGalleryDataForMediaObject(mediaObject, (IAlbum)mediaObject.Parent, new Entity.GalleryDataLoadOptions { LoadMediaItems = true }));
            }
            catch (InvalidMediaObjectException)
            {
                return NotFound("Media Asset Not Found: Could not find media object with ID {id}.");
            }
            catch (GallerySecurityException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex);

                return StatusCode(500, _exController.GetExString(ex));
            }
        }

        /// <summary>
        /// Gets the meta items for the specified media object <paramref name="id" />.
        /// </summary>
        /// <param name="id">The media object ID.</param>
        /// <returns>IEnumerable{Entity.MetaItem}.</returns>
        [HttpGet, ActionName("Meta")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMetaItemsForMediaObjectId(int id)
        {
            // GET /api/mediaitems/meta/12 - Gets metadata items for media object #12
            try
            {
                return new JsonResult(await _galleryObjectController.GetMetaItemsForMediaObject(id));
            }
            catch (InvalidMediaObjectException)
            {
                return NotFound("Media Asset Not Found: Could not find media object with ID {id}.");
            }
            catch (GallerySecurityException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex);

                return StatusCode(500, _exController.GetExString(ex));
            }
        }

        /// <summary>
        /// Adds a media file to an album. Prior to calling this method, the file should exist in the
        /// temporary upload directory (<see cref="GlobalConstants.TempUploadDirectory" />) in the
        /// App_Data directory with the name <see cref="AddMediaObjectSettings.FileNameOnServer" />. The
        /// file is copied to the destination album and given the name of
        /// <see cref="AddMediaObjectSettings.FileName" /> (instead of whatever name it currently has, which
        /// may contain a GUID).
        /// </summary>
        /// <param name="settings">The settings that contain data and configuration options for the media file.</param>
        /// <returns>List{ActionResult}.</returns>
        [HttpGet]
        public async Task<IActionResult> CreateFromFile(AddMediaObjectSettings settings)
        {
            try
            {
                settings.CurrentUserName = _userController.UserName;

                var fileExt = Path.GetExtension(settings.FileName);

                if (fileExt != null && fileExt.Equals(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    _galleryObjectController.AddMediaObject(settings);

                    //Task.Factory.StartNew(async () =>
                    //{
                    //    var results = await _galleryObjectController.AddMediaObject(settings);

                    //    // Since we don't have access to the user's session here, let's create a log entry.
                    //    //LogUploadZipFileResults(results, settings);
                    //});

                    return new JsonResult(new List<Business.ActionResult>
                    {
                        new Business.ActionResult
                        {
                            Title = settings.FileName,
                            Status = ActionResultStatus.Async.ToString()
                        }
                    });
                }
                else
                {
                    var results = await _galleryObjectController.AddMediaObject(settings);

                    //Utils.AddResultToSession(results);

                    return new JsonResult(results);
                }
            }
            catch (GallerySecurityException)
            {
                AppEventController.LogEvent(String.Format(CultureInfo.InvariantCulture, "Unauthorized access detected. The security system prevented a user from adding a media object."), null, EventType.Warning);

                return Forbid();
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex);

                return StatusCode(500, _exController.GetExString(ex));
            }
        }

        /// <summary>
        /// Replace the original file associated with <paramref name="mediaAssetId" /> with <paramref name="fileNameOnServer" />. Most metadata is copied from
        /// the current file to <paramref name="fileNameOnServer" />. Orientation meta, if present, is removed. Some meta properties are updated (e.g. width, height).
        /// Thumbnail and optimized images are regenerated. Requires the application be running in trial mode or under a license of Home &amp; Nonprofit or higher.
        /// </summary>
        /// <param name="mediaAssetId">The ID of the media asset.</param>
        /// <param name="fileNameOnServer">The full path to the edited file. Ex: "C:\Dev\GS\Dev-Main\Website\App_Data\_Temp\85b74137-d795-40a5-8b93-bf31de0b0ca3.jpg"</param>
        /// <returns>An instance of <see cref="IActionResult" />.</returns>
        [HttpPost]
        public async Task<IActionResult> ReplaceWithEditedImage(int mediaAssetId, string fileNameOnServer)
        {
            try
            {
                var filePath = Path.Combine(AppSetting.Instance.ContentRootPath, GlobalConstants.TempUploadDirectory, fileNameOnServer);

                return new JsonResult(await _galleryObjectController.ReplaceWithEditedImage(mediaAssetId, filePath));
            }
            catch (InvalidMediaObjectException ex)
            {
                return new JsonResult(new Business.ActionResult()
                {
                    Status = ActionResultStatus.Error.ToString(),
                    Title = "Cannot Edit Media Asset",
                    Message = ex.Message
                });
            }
            catch (GallerySecurityException ex)
            {
                return new JsonResult(new Business.ActionResult()
                {
                    Status = ActionResultStatus.Error.ToString(),
                    Title = "Cannot Edit Media Asset",
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex);

                return StatusCode(500, _exController.GetExString(ex));
            }
        }

        /// <summary>
        /// Replace the original file associated with <paramref name="mediaAssetId" /> with <paramref name="fileNameOnServer" /> and giving it the name 
        /// <paramref name="fileName" />. Re-extract relevant metadata such as width, height, orientation, video/audio info, etc. Thumbnail and 
        /// optimized images are regenerated. The original file is not modified.
        /// </summary>
        /// <param name="mediaAssetId">The ID of the media asset.</param>
        /// <param name="fileNameOnServer">The full path to the edited file. Ex: "C:\Dev\GS\Dev-Main\Website\App_Data\_Temp\85b74137-d795-40a5-8b93-bf31de0b0ca3.jpg"</param>
        /// <param name="fileName">Name the file should be given when it is persisted to the album directory.</param>
        /// <returns>An instance of <see cref="ActionResult" />.</returns>
        [HttpPost]
        public async Task<IActionResult> ReplaceMediaAssetFile(int mediaAssetId, string fileNameOnServer, string fileName)
        {
            try
            {
                var filePath = Path.Combine(AppSetting.Instance.ContentRootPath, GlobalConstants.TempUploadDirectory, fileNameOnServer);

                return new JsonResult(await _galleryObjectController.ReplaceMediaAssetFile(mediaAssetId, filePath, fileName));
            }
            catch (InvalidMediaObjectException ex)
            {
                return new JsonResult(new Business.ActionResult()
                {
                    Status = ActionResultStatus.Error.ToString(),
                    Title = "Cannot Edit Media Asset",
                    Message = ex.Message
                });
            }
            catch (GallerySecurityException ex)
            {
                return new JsonResult(new Business.ActionResult()
                {
                    Status = ActionResultStatus.Error.ToString(),
                    Title = "Cannot Replace Media Asset",
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex);

                return StatusCode(500, _exController.GetExString(ex));
            }
        }


        ///// <summary>
        ///// Persists the media item to the data store. The current implementation requires that
        ///// an existing item exist in the data store.
        ///// </summary>
        ///// <param name="mediaItem">An instance of <see cref="Entity.MediaItem"/> to persist to the data 
        ///// store.</param>
        ///// <exception cref="System.Web.Http.HttpResponseException"></exception>
        //public ActionResult PutMediaItem(Entity.MediaItem mediaItem)
        //{
        //	try
        //	{
        //		var mo = Factory.LoadMediaObjectInstance(new MediaLoadOptions(mediaItem.Id) { IsWritable = true });

        //		var isUserAuthorized = Utils.IsUserAuthorized(SecurityActions.EditMediaObject, mo.Parent.Id, mo.GalleryId, mo.IsPrivate, ((IAlbum)mo.Parent).IsVirtualAlbum);
        //		if (!isUserAuthorized)
        //		{
        //			AppEventController.LogEvent(String.Format(CultureInfo.InvariantCulture, "Unauthorized access detected. The security system prevented a user from editing media object {0}.", mo.Id), mo.GalleryId, EventType.Warning);

        //			throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
        //		}

        //		mo.Title = mediaItem.Title;
        //		GalleryObjectController.SaveGalleryObject(mo);

        //		return new ActionResult
        //		{
        //			Status = ActionResultStatus.Success.ToString(),
        //			Title = String.Empty,
        //			Message = String.Empty,
        //			ActionTarget = mediaItem
        //		};
        //	}
        //	catch (InvalidMediaObjectException)
        //	{
        //		throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
        //		{
        //			Content = new StringContent(String.Format("Could not find media item with ID = {0}", mediaItem.Id)),
        //			ReasonPhrase = "Media Object Not Found"
        //		});
        //	}
        //	catch (HttpResponseException)
        //	{
        //		throw; // Rethrow, since we've already logged it above
        //	}
        //	catch (Exception ex)
        //	{
        //		AppEventController.LogError(ex);

        //		throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
        //		{
        //			Content = Utils.GetExStringContent(ex),
        //			ReasonPhrase = "Server Error"
        //		});
        //	}
        //}

        /// <summary>
        /// Permanently deletes the specified media object from the file system and data store. No action is taken if the
        /// user does not have delete permission.
        /// </summary>
        /// <param name="id">The ID of the media object to be deleted.</param>
        //public HttpResponseMessage DeleteMediaItem(Entity.MediaItem mediaItem)
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            IGalleryObject mo = null;

            try
            {
                mo = Factory.LoadMediaObjectInstance(id);

                var isUserAuthorized = (await _authorizationService.AuthorizeAsync(User, mo, Operations.DeleteMediaObject)).Succeeded;
                //var isUserAuthorized = Utils.IsUserAuthorized(SecurityActions.DeleteMediaObject, mo.Parent.Id, mo.GalleryId, mo.IsPrivate, ((IAlbum)mo.Parent).IsVirtualAlbum);

                var isGalleryReadOnly = Factory.LoadGallerySetting(mo.GalleryId).MediaObjectPathIsReadOnly;
                if (!isUserAuthorized || isGalleryReadOnly)
                {
                    AppEventController.LogEvent($"Unauthorized access detected. The security system prevented a user from deleting media asset {mo.Id}.", mo.GalleryId, EventType.Warning);

                    return Forbid();
                }

                mo.Delete();

                return Ok($"Media asset {id} deleted...");
            }
            catch (InvalidMediaObjectException)
            {
                // HTTP specification says the DELETE method must be idempotent, so deleting a nonexistent item must have 
                // the same effect as deleting an existing one. So we do nothing here and let the method return HttpStatusCode.OK.
                return Ok($"Media asset with ID {id} does not exist.");
            }
            catch (Exception ex)
            {
                if (mo != null)
                    AppEventController.LogError(ex, mo.GalleryId);
                else
                    AppEventController.LogError(ex);

                return StatusCode(500, _exController.GetExString(ex));
            }
        }
    }
}
