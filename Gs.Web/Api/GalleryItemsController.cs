using GalleryServer.Business;
using GalleryServer.Web.Controller;
using GalleryServer.Web.Entity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GalleryServer.Web.Api
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class GalleryItemsController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly GalleryObjectController _galleryObjectController;
        private readonly ExceptionController _exController;

        public GalleryItemsController(GalleryObjectController galleryObjectController, ExceptionController exController)
        {
            _galleryObjectController = galleryObjectController;
            _exController = exController;
        }

        /// <summary>
        /// Permanently deletes the specified <paramref name="galleryItems" /> from the file system and data store. No action is taken if the
        /// user does not have delete permission. The successfully deleted items are assigned to the <see cref="Business.ActionResult.ActionTarget" />
        /// property of the returned instance.
        /// </summary>
        /// <param name="galleryItems">The gallery items to be deleted.</param>
        /// <param name="deleteFromFileSystem">if set to <c>true</c> [delete from file system].</param>
        /// <returns>An instance of <see cref="Business.ActionResult" />.</returns>
        [HttpDelete]
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(GalleryItem[] galleryItems, bool deleteFromFileSystem)
        {
            // DELETE galleryitems/delete
            try
            {
                return new JsonResult(await _galleryObjectController.DeleteGalleryItems(galleryItems, deleteFromFileSystem));
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex);

                return StatusCode(500, _exController.GetExString(ex));
            }
        }

        /// <summary>
        /// Permanently delete the original file for all <paramref name="galleryItems" />, including any children if a gallery item is an album.
        /// If no optimized version exists, no action is taken on that media asset. Validation is performed to ensure the logged in user has 
        /// permission to edit the items and that no business rules are violated. The successfully processed items are assigned to the 
        /// <see cref="Business.ActionResult.ActionTarget" /> property of the returned instance.
        /// </summary>
        /// <param name="galleryItems">The gallery items for which the original files are to be deleted.</param>
        /// <returns>An instance of <see cref="Business.ActionResult" /> describing the result of the deletion.</returns>
        [HttpDelete]
        [ActionName("DeleteOriginalFiles")]
        public async Task<IActionResult> DeleteOriginalFiles(GalleryItem[] galleryItems)
        {
            // DELETE galleryitems/deleteoriginalfiles
            try
            {
                return new JsonResult(await _galleryObjectController.DeleteOriginalFiles(galleryItems));
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex);

                return StatusCode(500, _exController.GetExString(ex));
            }
        }

        /// <summary>
        /// Executes the requested <paramref name="rotateFlip" /> action on the <paramref name="galleryItems" />. Validation is performed
        /// to ensure logged on user has <see cref="Business.SecurityActions.EditMediaObject" /> permission and that none of the items are in a read-only
        /// gallery.
        /// </summary>
        /// <param name="galleryItems">The gallery items to rotate or flip.</param>
        /// <param name="rotateFlip">The requested rotate / flip action.</param>
        /// <param name="viewSize">The size of the image the user is looking at.</param>
        /// <returns>An instance of <see cref="ActionResult" />.</returns>
        /// <exception cref="WebRequestMethods.Http.HttpResponseException"></exception>
        /// <exception cref="HttpResponseMessage"></exception>
        [HttpPost]
        [ActionName("RotateFlip")]
        public async Task<IActionResult> RotateFlip(GalleryItem[] galleryItems, MediaAssetRotateFlip rotateFlip, DisplayObjectType viewSize)
        {
            try
            {
                return new JsonResult(await _galleryObjectController.RotateFlip(galleryItems, rotateFlip, viewSize));
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex);

                return StatusCode(500, _exController.GetExString(ex));
            }
        }
    }
}
