using GalleryServer.Business;
using GalleryServer.Business.Metadata;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Controller;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GalleryServer.Web.Api
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class GalleryItemMetaController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly MetadataController _metadataController;
        private readonly ExceptionController _exController;

        public GalleryItemMetaController(MetadataController metadataController, ExceptionController exController)
        {
            _metadataController = metadataController;
            _exController = exController;
        }

        /// <summary>
        /// Gets the meta items for the specified <paramref name="galleryItems" />.
        /// </summary>
        /// <param name="galleryItems">An array of <see cref="Entity.GalleryItem" /> instances.</param>
        /// <returns>Returns a merged set of metadata.</returns>
        [HttpPost, ActionName("GalleryItems")]
        public async Task<IActionResult> GetMetaItemsForGalleryItems(Entity.GalleryItem[] galleryItems)
        {
            // GET /api/meta/galleryitems - Gets metadata items for the specified objects
            try
            {
                return new JsonResult(await _metadataController.GetMetaItemsForGalleryItems(galleryItems));
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
        /// Gets a value indicating whether the logged-on user has edit permission for all of the <paramref name="galleryItems" />.
        /// </summary>
        /// <param name="galleryItems">A collection of <see cref="Entity.GalleryItem" /> instances.</param>
        /// <returns><c>true</c> if the current user can edit the items; <c>false</c> otherwise.</returns>
        [HttpPost]
        public IActionResult CanUserEdit(System.Collections.Generic.IEnumerable<Entity.GalleryItem> galleryItems)
        {
            // POST /api/meta/canuseredit
            try
            {
                return Ok(_metadataController.CanUserEditAllItems(galleryItems));
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex);

                return StatusCode(500, _exController.GetExString(ex));
            }
        }

        /// <summary>
        /// Updates the gallery items with the specified metadata value. <see cref="Business.ActionResult" />
        /// contains details about the success or failure of the operation.
        /// </summary>
        /// <param name="galleryItemMeta">An instance of <see cref="Entity.GalleryItemMeta" /> that defines
        /// the tag value to be added and the gallery items it is to be added to. It is expected that only
        /// the MTypeId and Value properties of <see cref="Entity.GalleryItemMeta.MetaItem" /> are populated.</param>
        /// <returns>An instance of <see cref="Business.ActionResult" />.</returns>
        [HttpPut]
        public IActionResult PutGalleryItemMeta(Entity.GalleryItemMeta galleryItemMeta)
        {
            // PUT /api/galleryitemmeta
            try
            {
                _metadataController.SaveGalleryItemMeta(galleryItemMeta);

                if (galleryItemMeta.ActionResult == null)
                {
                    galleryItemMeta.ActionResult = new Business.ActionResult()
                    {
                        Status = ActionResultStatus.Success.ToString(),
                        Title = "Save successful"
                    };
                }
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

            return Ok(galleryItemMeta);
        }

        /// <summary>
        /// Deletes the meta tag value from the specified gallery items.
        /// </summary>
        /// <param name="galleryItemMeta">An instance of <see cref="Entity.GalleryItemMeta" /> that defines
        /// the tag value to be added and the gallery items it is to be added to.</param>
        /// <returns><see cref="HttpResponseMessage" />.</returns>
        [HttpDelete]
        public async Task<IActionResult> DeleteGalleryItemMeta(Entity.GalleryItemMeta galleryItemMeta)
        {
            // DELETE /api/galleryitemmeta
            try
            {
                var mType = (MetadataItemName)galleryItemMeta.MetaItem.MTypeId;
                if (mType == MetadataItemName.Tags || mType == MetadataItemName.People)
                {
                    await _metadataController.DeleteTag(galleryItemMeta);
                }
                else
                {
                    await _metadataController.Delete(galleryItemMeta);
                }

                return Ok("Meta item deleted...");
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
    }
}
