using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using GalleryServer.Business;
using GalleryServer.Business.Metadata;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Controller;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GalleryServer.Web.Api
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MetaController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly MetadataController _metadataController;
        private readonly UserController _userController;
        private readonly ExceptionController _exController;

        public MetaController(MetadataController metadataController, UserController userController, ExceptionController exController)
        {
            _metadataController = metadataController;
            _userController = userController;
            _exController = exController;
        }

        #region Methods

        /// <summary>
        /// Gets a list of tags the current user can view. Guaranteed to not return null.
        /// </summary>
        /// <param name="q">The search term. Only tags that begin with this string are returned.
        /// Specify null or an empty string to return all tags.</param>
        /// <param name="galleryId">The gallery ID.</param>
        /// <param name="top">The number of tags to return. Values less than zero are treated the same as zero,
        /// meaning no tags will be returned. Specify <see cref="int.MaxValue" /> to return all tags.</param>
        /// <param name="sortBy">The property to sort the tags by. Specify "count" to sort by tag frequency or
        /// "value" to sort by tag name. When not specified, defaults to "notspecified".</param>
        /// <param name="sortAscending">Specifies whether to sort the tags in ascending order. Specify <c>true</c>
        /// for ascending order or <c>false</c> for descending order. When not specified, defaults to <c>false</c>.</param>
        /// <returns>IEnumerable{Business.Entity.Tag}.</returns>
        [HttpGet, ActionName("Tags")]
        public async Task<IActionResult> GetTags(string q, int galleryId, int top = int.MaxValue, string sortBy = "notspecified", bool sortAscending = false)
        {
            try
            {
                if (!Enum.TryParse(sortBy, true, out TagSearchOptions.TagProperty sortProperty))
                {
                    sortProperty = TagSearchOptions.TagProperty.NotSpecified;
                }

                return new JsonResult(await _metadataController.GetTags(TagSearchType.TagsUserCanView, q, galleryId, top, sortProperty, sortAscending));
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex);

                return StatusCode(500, _exController.GetExString(ex));
            }
        }

        /// <summary>
        /// Gets a JSON string representing the tags used in the specified gallery. The JSON can be used as the
        /// data source for the jsTree jQuery widget. Only tags the current user has permission to view are
        /// included. The tag tree has a root node containing a single level of tags. Throws an exception when
        /// the application is not running an Enterprise License.
        /// </summary>
        /// <param name="galleryId">The gallery ID.</param>
        /// <param name="top">The number of tags to return. Values less than zero are treated the same as zero,
        /// meaning no tags will be returned. Specify <see cref="int.MaxValue" /> to return all tags.</param>
        /// <param name="sortBy">The property to sort the tags by. Specify "count" to sort by tag frequency or
        /// "value" to sort by tag name. When not specified, defaults to "count".</param>
        /// <param name="sortAscending">Specifies whether to sort the tags in ascending order. Specify <c>true</c>
        /// for ascending order or <c>false</c> for descending order. When not specified, defaults to <c>false</c>.</param>
        /// <param name="expanded">if set to <c>true</c> the tree is configured to display in an expanded form.</param>
        /// <returns>System.String.</returns>
        [HttpGet]
        public async Task<IActionResult> GetTagTreeAsJson(int galleryId, int top = int.MaxValue, string sortBy = "count", bool sortAscending = false, bool expanded = false)
        {
            try
            {
                //ValidateEnterpriseLicense();

                if (!Enum.TryParse(sortBy, true, out TagSearchOptions.TagProperty sortProperty))
                {
                    sortProperty = TagSearchOptions.TagProperty.NotSpecified;
                }

                return new JsonResult(await _metadataController.GetTagTreeAsJson(TagSearchType.TagsUserCanView, galleryId, top, sortProperty, sortAscending, expanded));
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
        /// Gets a JSON string representing the tags used in the specified gallery. The JSON can be used as the 
        /// data source for the jsTree jQuery widget. Only tags the current user has permission to view are
        /// included. The tag tree has a root node containing a single level of tags. Throws an exception when
        /// the application is not running an Enterprise License.
        /// </summary>
        /// <param name="galleryId">The gallery ID.</param>
        /// <param name="top">The number of tags to return. Values less than zero are treated the same as zero,
        /// meaning no tags will be returned. Specify <see cref="int.MaxValue" /> to return all tags.</param>
        /// <param name="sortBy">The property to sort the tags by. Specify "count" to sort by tag frequency or 
        /// "value" to sort by tag name. When not specified, defaults to "count".</param>
        /// <param name="sortAscending">Specifies whether to sort the tags in ascending order. Specify <c>true</c>
        /// for ascending order or <c>false</c> for descending order. When not specified, defaults to <c>false</c>.</param>
        /// <param name="expanded">if set to <c>true</c> the tree is configured to display in an expanded form.</param>
        /// <returns>System.String.</returns>
        [HttpGet]
        public async Task<IActionResult> GetPeopleTreeAsJson(int galleryId, int top = int.MaxValue, string sortBy = "count", bool sortAscending = false, bool expanded = false)
        {
            try
            {
                if (!Enum.TryParse(sortBy, true, out TagSearchOptions.TagProperty sortProperty))
                {
                    sortProperty = TagSearchOptions.TagProperty.NotSpecified;
                }

                return new JsonResult(await _metadataController.GetTagTreeAsJson(TagSearchType.PeopleUserCanView, galleryId, top, sortProperty, sortAscending, expanded));
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
        /// Gets a list of people the current user can view. Guaranteed to not return null.
        /// </summary>
        /// <param name="q">The search term. Only tags that begin with this string are returned.
        /// Specify null or an empty string to return all tags.</param>
        /// <param name="galleryId">The gallery ID.</param>
        /// <param name="top">The number of tags to return. Values less than zero are treated the same as zero,
        /// meaning no tags will be returned. Specify <see cref="int.MaxValue" /> to return all tags.</param>
        /// <param name="sortBy">The property to sort the tags by. Specify "count" to sort by tag frequency or
        /// "value" to sort by tag name. When not specified, defaults to "notspecified".</param>
        /// <param name="sortAscending">Specifies whether to sort the tags in ascending order. Specify <c>true</c>
        /// for ascending order or <c>false</c> for descending order. When not specified, defaults to <c>false</c>.</param>
        /// <returns>IEnumerable{Business.Entity.Tag}.</returns>
        [HttpGet, ActionName("People")]
        public async Task<IActionResult> GetPeople(string q, int galleryId, int top = int.MaxValue, string sortBy = "notspecified", bool sortAscending = false)
        {
            try
            {
                if (!Enum.TryParse(sortBy, true, out TagSearchOptions.TagProperty sortProperty))
                {
                    sortProperty = TagSearchOptions.TagProperty.NotSpecified;
                }

                return new JsonResult(await _metadataController.GetTags(TagSearchType.PeopleUserCanView, q, galleryId, top, sortProperty, sortAscending));
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex);

                return StatusCode(500, _exController.GetExString(ex));
            }
        }

        /// <summary>
        /// Persists the metadata item to the data store. The current implementation requires that
        /// an existing item exist in the data store and only stores the contents of the
        /// <see cref="Entity.MetaItem.Value" /> property.
        /// </summary>
        /// <param name="metaItem">An instance of <see cref="Entity.MetaItem" /> to persist to the data
        /// store.</param>
        /// <returns>Entity.MetaItem.</returns>
        [HttpPut]
        public async Task<IActionResult> PutMetaItem(Entity.MetaItem metaItem)
        {
            try
            {
                return new JsonResult(await _metadataController.Save(metaItem));
            }
            catch (InvalidAlbumException)
            {
                return NotFound($"Could not find album with ID {metaItem.MediaId}");
            }
            catch (InvalidMediaObjectException)
            {
                return NotFound($"Media Asset/Metadata Item Not Found: One of the following errors occurred: (1) Could not find meta item with ID {metaItem.Id} (2) Could not find media asset with ID {metaItem.MediaId}");
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
        /// Writes database metadata having ID <paramref name="metaNameId" /> to the media file for all writable assets in the gallery having ID 
        /// <paramref name="galleryId" />. The action is executed asynchronously and returns immediately.
        /// </summary>
        /// <param name="metaNameId">ID of the meta item. This must match the enumeration value of <see cref="MetadataItemName" />.</param>
        /// <param name="galleryId">The gallery ID.</param>
        [HttpPost]
        [ActionName("WriteMetaItem")]
        public async Task<IActionResult> WriteItemForGallery(int metaNameId, int galleryId)
        {
            try
            {
                if (await _userController.IsCurrentUserGalleryAdministrator(galleryId))
                {
                    var metaName = (MetadataItemName)metaNameId;
                    if (MetadataItemNameEnumHelper.IsValidFormattedMetadataItemName(metaName))
                    {
                        _metadataController.WriteItemForGalleryAsync(metaName, galleryId);
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex);

                return StatusCode(500, _exController.GetExString(ex));
            }
        }

        /// <summary>
        /// Rebuilds the meta name having ID <paramref name="metaNameId" /> for all items in the gallery having ID 
        /// <paramref name="galleryId" />. The action is executed asynchronously and returns immediately.
        /// </summary>
        /// <param name="metaNameId">ID of the meta item. This must match the enumeration value of <see cref="MetadataItemName" />.</param>
        /// <param name="galleryId">The gallery ID.</param>
        [HttpPost]
        [ActionName("RebuildMetaItem")]
        public async Task<IActionResult> RebuildItemForGallery(int metaNameId, int galleryId)
        {
            try
            {
                if (await _userController.IsCurrentUserGalleryAdministrator(galleryId))
                {
                    var metaName = (MetadataItemName)metaNameId;
                    if (MetadataItemNameEnumHelper.IsValidFormattedMetadataItemName(metaName))
                    {
                        _metadataController.RebuildItemForGalleryAsync(metaName, galleryId);
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex);

                return StatusCode(500, _exController.GetExString(ex));
            }
        }

        #endregion

        #region Functions

        ///// <summary>
        ///// Verifies the application is running an Enterprise License, throwing a <see cref="GallerySecurityException" />
        ///// if it is not.
        ///// </summary>
        ///// <exception cref="GallerySecurityException">Thrown when the application is not running an Enterprise License.
        ///// </exception>
        //private static void ValidateEnterpriseLicense()
        //{
        //	if (AppSetting.Instance.License.LicenseType != LicenseLevel.Enterprise)
        //	{
        //		AppEventController.LogEvent("Attempt to use a feature that requires an Enterprise License.", null, EventType.Warning);

        //		throw new GallerySecurityException("Attempt to use a feature that requires an Enterprise License.");
        //	}
        //}

        // WARNING: Given the current API, there is no way to verify the user has permission to 
        // view the specified meta ID, so we'll comment out this method to ensure it isn't used.
        ///// <summary>
        ///// Gets the meta item with the specified <paramref name="id" />.
        ///// Example: api/meta/4/
        ///// </summary>
        ///// <param name="id">The value that uniquely identifies the metadata item.</param>
        ///// <returns>An instance of <see cref="Entity.MetaItem" />.</returns>
        ///// <exception cref="System.Web.Http.HttpResponseException"></exception>
        //public Entity.MetaItem Get(int id)
        //{
        //	try
        //	{
        //		return MetadataController.Get(id);
        //	}
        //	catch (InvalidMediaObjectException)
        //	{
        //		throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
        //		{
        //			Content = new StringContent(String.Format("Could not find meta item with ID = {0}", id)),
        //			ReasonPhrase = "Media Object Not Found"
        //		});
        //	}
        //	catch (GallerySecurityException)
        //	{
        //		throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Forbidden));
        //	}
        //}

        #endregion
    }
}
