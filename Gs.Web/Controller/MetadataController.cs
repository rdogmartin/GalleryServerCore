using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GalleryServer.Business;
using GalleryServer.Business.Entity;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Metadata;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Web.Entity;
using Microsoft.AspNetCore.Mvc;
using ActionResult = GalleryServer.Business.ActionResult;

namespace GalleryServer.Web.Controller
{
    /// <summary>
    /// Contains functionality for interacting with metadata.
    /// </summary>
    public class MetadataController
    {
        private readonly AlbumController _albumController;
        private readonly GalleryObjectController _galleryObjectController;
        private readonly UserController _userController;

        public MetadataController(AlbumController albumController, GalleryObjectController galleryObjectController, UserController userController)
        {
            _albumController = albumController;
            _galleryObjectController = galleryObjectController;
            _userController = userController;
        }

        #region Methods

        /// <summary>
        /// Gets the meta item with the specified <paramref name="id" />. Since the current API
        /// cannot efficient look up the gallery object ID and type, those are included as required
        /// parameters. They are assigned to the corresponding properties of the returned instance.
        /// Verifies user has permission to view item, throwing <see cref="GallerySecurityException" /> 
        /// if authorization is denied.
        /// </summary>
        /// <param name="id">The value that uniquely identifies the metadata item.</param>
        /// <param name="galleryObjectId">The gallery object ID. It is assigned to 
        /// <see cref="Entity.MetaItem.MediaId" />.</param>
        /// <param name="goType">Type of the gallery object. It is assigned to 
        /// <see cref="Entity.MetaItem.GTypeId" />.</param>
        /// <returns>An instance of <see cref="Entity.MetaItem" />.</returns>
        /// <exception cref="GallerySecurityException">Thrown when user does not have permission to
        /// view the requested item.</exception>
        /// <exception cref="InvalidMediaObjectException">Thrown when the requested meta item or its
        /// associated media object does not exist in the data store.</exception>
        /// <exception cref="InvalidAlbumException">Thrown when the album associated with the
        /// meta item does not exist in the data store.</exception>
        public async Task<MetaItem> Get(int id, int galleryObjectId, GalleryObjectType goType)
        {
            var md = Factory.LoadGalleryObjectMetadataItem(id);
            if (md == null)
                throw new InvalidMediaObjectException(String.Format("No metadata item with ID {0} could be found in the data store.", id));

            // Security check: Make sure user has permission to view item
            IGalleryObject go;
            if (goType == GalleryObjectType.Album)
            {
                go = Factory.LoadAlbumInstance(galleryObjectId);
                SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.ViewAlbumOrMediaObject, await _userController.GetGalleryServerRolesForUser(), go.Id, go.GalleryId, _userController.IsAuthenticated, go.IsPrivate, ((IAlbum)go).IsVirtualAlbum);
            }
            else
            {
                go = Factory.LoadMediaObjectInstance(galleryObjectId);
                SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.ViewAlbumOrMediaObject, await _userController.GetGalleryServerRolesForUser(), go.Parent.Id, go.GalleryId, _userController.IsAuthenticated, go.Parent.IsPrivate, ((IAlbum)go.Parent).IsVirtualAlbum);
            }

            var metaDef = Factory.LoadGallerySetting(go.GalleryId).MetadataDisplaySettings.Find(md.MetadataItemName);

            // The HTML editor requires the trial version or Home & Nonprofit or higher.
            var editMode = (metaDef.UserEditMode == PropertyEditorMode.TinyMCEHtmlEditor && AppSetting.Instance.License.LicenseType < LicenseLevel.HomeNonprofit ? PropertyEditorMode.PlainTextEditor : metaDef.UserEditMode);

            return new Entity.MetaItem
            {
                Id = md.MediaObjectMetadataId,
                MediaId = galleryObjectId,
                MTypeId = (int)md.MetadataItemName,
                GTypeId = (int)goType,
                Desc = md.Description,
                Value = md.Value,
                //IsEditable = metaDef.IsEditable,
                EditMode = editMode
            };
        }

        /// <summary>
        /// Gets the requested <paramref name="metaName" /> instance for the specified <paramref name="galleryObjectId" />
        /// having the specified <paramref name="goType" />. Returns null if no metadata item exists.
        /// Verifies user has permission to view item, throwing <see cref="GallerySecurityException" /> 
        /// if authorization is denied.
        /// </summary>
        /// <param name="galleryObjectId">The ID for either an album or a media object.</param>
        /// <param name="goType">The type of gallery object.</param>
        /// <param name="metaName">Name of the metaitem to return.</param>
        /// <returns>Returns an instance of <see cref="IGalleryObjectMetadataItemCollection" />.</returns>
        /// <exception cref="GallerySecurityException">Thrown when user does not have permission to
        /// view the requested item.</exception>
        /// <exception cref="InvalidMediaObjectException">Thrown when the requested meta item or its
        /// associated media object does not exist in the data store.</exception>
        /// <exception cref="InvalidAlbumException">Thrown when the album associated with the
        /// meta item does not exist in the data store.</exception>
        public async Task<IGalleryObjectMetadataItem> Get(int galleryObjectId, GalleryObjectType goType, MetadataItemName metaName)
        {
            // Security check: Make sure user has permission to view item
            IGalleryObject go;
            if (goType == GalleryObjectType.Album)
            {
                go = Factory.LoadAlbumInstance(galleryObjectId);
                SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.ViewAlbumOrMediaObject, await _userController.GetGalleryServerRolesForUser(), go.Id, go.GalleryId, _userController.IsAuthenticated, go.IsPrivate, ((IAlbum)go).IsVirtualAlbum);
            }
            else
            {
                go = Factory.LoadMediaObjectInstance(galleryObjectId);
                SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.ViewAlbumOrMediaObject, await _userController.GetGalleryServerRolesForUser(), go.Parent.Id, go.GalleryId, _userController.IsAuthenticated, go.Parent.IsPrivate, ((IAlbum)go.Parent).IsVirtualAlbum);
            }

            IGalleryObjectMetadataItem md;
            GetGalleryObjectMetadataItemCollection(galleryObjectId, goType).TryGetMetadataItem(metaName, out md);

            return md;
        }

        /// <summary>
        /// Gets the meta items for specified <paramref name="galleryItems" />, merging metadata
        /// when necessary. Specifically, tags and people tags are merged and updated with a count.
        /// Example: "Animal (3), Dog (2), Cat (1)" indicates three of the gallery items have the 
        /// 'Animal' tag, two have the 'Dog' tag, and one has the 'Cat' tag. Guaranteed to not 
        /// return null.
        /// </summary>
        /// <param name="galleryItems">The gallery items for which to retrieve metadata.</param>
        /// <returns>Returns a collection of <see cref="Entity.MetaItem" /> items.</returns>
        /// <exception cref="InvalidAlbumException">Thrown when the requested album does not exist.</exception>
        /// <exception cref="InvalidMediaObjectException">Thrown when the requested media object does not exist.</exception>
        public async Task<IEnumerable<MetaItem>> GetMetaItemsForGalleryItems(Entity.GalleryItem[] galleryItems)
        {
            if (galleryItems == null || galleryItems.Length == 0)
                return new Entity.MetaItem[] { };

            var tagNames = new[] { MetadataItemName.Tags, MetadataItemName.People };
            var tagValues = new string[tagNames.Length]; // Eventually will contain the merged tag values

            // Iterate through each tag type and generate the merge tag value.
            for (int i = 0; i < tagNames.Length; i++)
            {
                var tags = GetTagListForGalleryItems(galleryItems, tagNames[i]);

                tagValues[i] = GetTagsWithCount(tags);
            }

            // Get the metadata for the last item and merge our computed tags into it
            var lastGi = galleryItems[galleryItems.Length - 1];
            var meta = await GetMetaItems(lastGi);

            for (int i = 0; i < tagValues.Length; i++)
            {
                var tagMi = GetMetaItemForTag(meta, tagNames[i], lastGi);
                tagMi.Value = tagValues[i];
            }

            return meta;
        }

        /// <summary>
        /// Persists the metadata item to the data store.  Verifies user has permission to edit item,
        /// throwing <see cref="GallerySecurityException" /> if authorization is denied. 
        /// The value is validated before saving, and may be altered to conform to business rules, 
        /// such as removing HTML tags and javascript. The <paramref name="metaItem" /> is returned,
        /// with the validated value assigned to the <see cref="Entity.MetaItem.Value" /> property.
        /// 
        /// The current implementation requires that an existing item exist in the data store and only 
        /// stores the contents of the <see cref="Entity.MetaItem.Value" /> property.
        /// </summary>
        /// <param name="metaItem">An instance of <see cref="Entity.MetaItem" /> to persist to the data
        /// store.</param>
        /// <returns>An instance of <see cref="Entity.MetaItem" />.</returns>
        /// <exception cref="InvalidMediaObjectException">Thrown when the requested meta item or its
        /// associated media object does not exist in the data store.</exception>
        /// <exception cref="GallerySecurityException">Thrown when user does not have permission to
        /// edit the requested item.</exception>
        public async Task<MetaItem> Save(Entity.MetaItem metaItem)
        {
            var md = Factory.LoadGalleryObjectMetadataItem(metaItem.Id);
            if (md == null)
                throw new InvalidMediaObjectException(String.Format("No metadata item with ID {0} could be found in the data store.", metaItem.Id));

            // Security check: Make sure user has permission to edit item
            IGalleryObject go;
            if (metaItem.GTypeId == (int)GalleryObjectType.Album)
            {
                go = Factory.LoadAlbumInstance(metaItem.MediaId);
                SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.EditAlbum, await _userController.GetGalleryServerRolesForUser(), go.Id, go.GalleryId, _userController.IsAuthenticated, go.IsPrivate, ((IAlbum)go).IsVirtualAlbum);
            }
            else
            {
                go = Factory.LoadMediaObjectInstance(metaItem.MediaId);
                SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.EditMediaObject, await _userController.GetGalleryServerRolesForUser(), go.Parent.Id, go.GalleryId, _userController.IsAuthenticated, go.Parent.IsPrivate, ((IAlbum)go.Parent).IsVirtualAlbum);
            }

            string prevValue = md.Value;

            md.Value = Utils.CleanHtmlTags(metaItem.Value, go.GalleryId);

            if (md.Value != prevValue)
            {
                Factory.SaveGalleryObjectMetadataItem(md, _userController.UserName);

                CacheController.PurgeCache(md.GalleryObject);
            }

            return metaItem;
        }

        /// <summary>
        /// Permanently deletes the meta item from the specified gallery items.
        /// </summary>
        /// <param name="galleryItemTag">An instance of <see cref="Entity.GalleryItemMeta" /> containing the 
        /// meta item to be deleted and the gallery items from which the item is to be deleted.</param>
        /// <exception cref="GallerySecurityException">Thrown when the current user does not have
        /// permission to edit one of the specified gallery items.</exception>
        public async Task Delete(Entity.GalleryItemMeta galleryItemTag)
        {
            foreach (var gi in galleryItemTag.GalleryItems)
            {
                IGalleryObject go;
                try
                {
                    if (gi.ItemType == (int)GalleryObjectType.Album)
                    {
                        go = Factory.LoadAlbumInstance(gi.Id);
                        SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.EditAlbum, await _userController.GetGalleryServerRolesForUser(), go.Id, go.GalleryId, _userController.IsAuthenticated, go.IsPrivate, ((IAlbum)go).IsVirtualAlbum);
                    }
                    else
                    {
                        go = Factory.LoadMediaObjectInstance(gi.Id);
                        SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.EditMediaObject, await _userController.GetGalleryServerRolesForUser(), go.Parent.Id, go.GalleryId, _userController.IsAuthenticated, go.Parent.IsPrivate, ((IAlbum)go.Parent).IsVirtualAlbum);
                    }
                }
                catch (InvalidAlbumException ex)
                {
                    AppEventController.LogError(ex);
                    continue;
                }
                catch (InvalidMediaObjectException ex)
                {
                    AppEventController.LogError(ex);
                    continue;
                }

                IGalleryObjectMetadataItem md;
                go.MetadataItems.TryGetMetadataItem((MetadataItemName)galleryItemTag.MetaItem.MTypeId, out md);

                if (md != null)
                {
                    md.IsDeleted = true;

                    Factory.SaveGalleryObjectMetadataItem(md, _userController.UserName);

                    CacheController.PurgeCache(go);
                }
            }
        }

        /// <summary>
        /// Updates the specified gallery items with the specified metadata value. The property <see cref="Entity.GalleryItemMeta.ActionResult" />
        /// of <paramref name="galleryItemMeta" /> is assigned when a validation error occurs, but remains null for a successful operation.
        /// </summary>
        /// <param name="galleryItemMeta">An object containing the metadata instance to use as the source
        /// and the gallery items to be updated</param>
        /// <exception cref="GallerySecurityException">Thrown when the current user does not have
        /// permission to edit one or more of the specified gallery items.</exception>
        public void SaveGalleryItemMeta(Entity.GalleryItemMeta galleryItemMeta)
        {
            var metaName = (MetadataItemName)galleryItemMeta.MetaItem.MTypeId;
            if (metaName == MetadataItemName.Tags || metaName == MetadataItemName.People)
            {
                AddTag(galleryItemMeta);
            }
            else if (metaName == MetadataItemName.Rating)
            {
                PersistRating(galleryItemMeta);
            }
            else
            {
                PersistGalleryItemMeta(galleryItemMeta);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the logged-on user has edit permission for all of the <paramref name="galleryItems" />.
        /// </summary>
        /// <param name="galleryItems">A collection of <see cref="Entity.GalleryItem" /> instances.</param>
        /// <returns><c>true</c> if the current user can edit the items; <c>false</c> otherwise.</returns>
        public bool CanUserEditAllItems(IEnumerable<Entity.GalleryItem> galleryItems)
        {
            try
            {
                foreach (var galleryItem in galleryItems)
                {
                    GetGalleryObjectAndVerifyEditPermission(galleryItem);
                }

                return true;
            }
            catch (GallerySecurityException)
            {
                return false;
            }
        }

        /// <summary>
        /// Deletes the tags from the specified gallery items. This method is intended only for tag-style
        /// metadata items, such as descriptive tags and people. It is assumed the metadata item in
        /// the data store is a comma-separated list of tags, and the passed in to this method is to 
        /// be removed from it. No action is taken on a gallery object if the tag already exists or the
        /// specified gallery object does not exist.
        /// </summary>
        /// <param name="galleryItemTag">An instance of <see cref="Entity.GalleryItemMeta" /> containing the tag
        /// and the gallery items the tag is to be removed from.</param>
        /// <exception cref="GallerySecurityException">Thrown when the current user does not have
        /// permission to edit one of the specified gallery items.</exception>
        public async Task DeleteTag(Entity.GalleryItemMeta galleryItemTag)
        {
            foreach (var gi in galleryItemTag.GalleryItems)
            {
                IGalleryObject go;
                try
                {
                    if (gi.ItemType == (int)GalleryObjectType.Album)
                    {
                        go = Factory.LoadAlbumInstance(gi.Id);
                        SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.EditAlbum, await _userController.GetGalleryServerRolesForUser(), go.Id, go.GalleryId, _userController.IsAuthenticated, go.IsPrivate, ((IAlbum)go).IsVirtualAlbum);
                    }
                    else
                    {
                        go = Factory.LoadMediaObjectInstance(gi.Id);
                        SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.EditMediaObject, await _userController.GetGalleryServerRolesForUser(), go.Parent.Id, go.GalleryId, _userController.IsAuthenticated, go.Parent.IsPrivate, ((IAlbum)go.Parent).IsVirtualAlbum);
                    }
                }
                catch (InvalidAlbumException ex)
                {
                    AppEventController.LogError(ex);
                    continue;
                }
                catch (InvalidMediaObjectException ex)
                {
                    AppEventController.LogError(ex);
                    continue;
                }

                IGalleryObjectMetadataItem md;
                go.MetadataItems.TryGetMetadataItem((MetadataItemName)galleryItemTag.MetaItem.MTypeId, out md);

                // Split tag into array, add it if it's not already there, and save
                var tags = md.Value.Split(new string[] { ", ", "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (tags.Contains(galleryItemTag.MetaItem.Value, StringComparer.OrdinalIgnoreCase))
                {
                    tags.Remove(galleryItemTag.MetaItem.Value);

                    md.Value = String.Join(", ", tags);

                    Factory.SaveGalleryObjectMetadataItem(md, _userController.UserName);

                    CacheController.PurgeCache(go);
                }
            }
        }

        /// <summary>
        /// Writes database metadata for <paramref name="metaName" /> to the media files for all writable assets in the gallery having ID 
        /// <paramref name="galleryId" />. The action is executed asynchronously and returns immediately.
        /// </summary>
        /// <param name="metaName">Name of the meta item.</param>
        /// <param name="galleryId">The gallery ID.</param>
        public void WriteItemForGalleryAsync(MetadataItemName metaName, int galleryId)
        {
            if (AppSetting.Instance.License.LicenseType < LicenseLevel.Enterprise)
            {
                throw new Exception("Batch metadata writing to the media files requires Gallery Server Enterprise or higher. To unlock this feature, enter a qualifying license key.");
            }

            var userName = _userController.UserName;
            var album = Factory.LoadRootAlbumInstance(galleryId);
            var gallerySettings = Factory.LoadGallerySetting(galleryId);

            if (gallerySettings.MediaObjectPathIsReadOnly)
            {
                throw new GallerySecurityException("Cannot modify original media files when the gallery is read only.");
            }

            var metaDefs = gallerySettings.MetadataDisplaySettings;
            var metaDef = metaDefs.Find(metaName);

            if (metaDef.IsPersistable)
            {
                Task.Factory.StartNew(() => StartWriteMetaItem(metaDefs.Find(metaName), album, userName), TaskCreationOptions.LongRunning);
            }
            else
            {
                throw new Exception($"The metadata property {metaName} cannot be written to the media file.");
            }
        }

        /// <summary>
        /// Rebuilds the <paramref name="metaName" /> for all items in the gallery having ID <paramref name="galleryId" />.
        /// The action is executed asynchronously and returns immediately.
        /// </summary>
        /// <param name="metaName">Name of the meta item.</param>
        /// <param name="galleryId">The gallery ID.</param>
        public void RebuildItemForGalleryAsync(MetadataItemName metaName, int galleryId)
        {
            var album = Factory.LoadRootAlbumInstance(galleryId);
            var metaDefs = Factory.LoadGallerySetting(galleryId).MetadataDisplaySettings;
            var userName = _userController.UserName;

            Task.Factory.StartNew(() => StartRebuildMetaItem(metaDefs.Find(metaName), album, userName), TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Gets a list of tags or people corresponding to the specified parameters.
        /// Guaranteed to not return null.
        /// </summary>
        /// <param name="tagSearchType">Type of the search.</param>
        /// <param name="searchTerm">The search term. Only tags that begin with this string are returned.
        /// Specify null or an empty string to return all tags.</param>
        /// <param name="galleryId">The gallery ID.</param>
        /// <param name="top">The number of tags to return. Values less than zero are treated the same as zero,
        /// meaning no tags will be returned. Specify <see cref="int.MaxValue" /> to return all tags.</param>
        /// <param name="sortBy">The property to sort the tags by. Specify <see cref="TagSearchOptions.TagProperty.Count" />
        /// to sort by tag frequency or <see cref="TagSearchOptions.TagProperty.Value" /> to sort by tag name. 
        /// When not specified, defaults to <see cref="TagSearchOptions.TagProperty.NotSpecified" />.</param>
        /// <param name="sortAscending">Specifies whether to sort the tags in ascending order. Specify <c>true</c>
        /// for ascending order or <c>false</c> for descending order. When not specified, defaults to <c>false</c>.</param>
        /// <returns>IEnumerable{Business.Entity.Tag}.</returns>
        public async Task<IEnumerable<Tag>> GetTags(TagSearchType tagSearchType, string searchTerm, int galleryId, int top = int.MaxValue, TagSearchOptions.TagProperty sortBy = TagSearchOptions.TagProperty.NotSpecified, bool sortAscending = false)
        {
            return GetTags(await GetTagSearchOptions(tagSearchType, searchTerm, galleryId, top, sortBy, sortAscending));
        }

        /// <summary>
        /// Gets a list of tags or people corresponding to the specified <paramref name="searchOptions" />.
        /// Guaranteed to not return null.
        /// </summary>
        /// <param name="searchOptions">The search options.</param>
        /// <returns>IEnumerable{Tag}.</returns>
        private IEnumerable<Business.Entity.Tag> GetTags(TagSearchOptions searchOptions)
        {
            var searcher = new TagSearcher(searchOptions);

            return searcher.Find();
        }

        /// <summary>
        /// Gets a JSON string representing the tags used in the specified gallery. The JSON can be used as the
        /// data source for the jsTree jQuery widget. Only tags the current user has permission to view are
        /// included. The tag tree has a root node containing a single level of tags.
        /// </summary>
        /// <param name="tagSearchType">Type of search.</param>
        /// <param name="galleryId">The gallery ID.</param>
        /// <param name="top">The number of tags to return. Values less than zero are treated the same as zero,
        /// meaning no tags will be returned. Specify <see cref="int.MaxValue" /> to return all tags.</param>
        /// <param name="sortBy">The property to sort the tags by. Specify <see cref="TagSearchOptions.TagProperty.Count" />
        /// to sort by tag frequency or <see cref="TagSearchOptions.TagProperty.Value" /> to sort by tag name. 
        /// When not specified, defaults to <see cref="TagSearchOptions.TagProperty.Count" />.</param>
        /// <param name="sortAscending">Specifies whether to sort the tags in ascending order. Specify <c>true</c>
        /// for ascending order or <c>false</c> for descending order. When not specified, defaults to <c>false</c>.</param>
        /// <param name="expanded">if set to <c>true</c> the tree is configured to display in an expanded form.</param>
        /// <returns>System.String.</returns>
        public async Task<string> GetTagTreeAsJson(TagSearchType tagSearchType, int galleryId, int top = int.MaxValue, TagSearchOptions.TagProperty sortBy = TagSearchOptions.TagProperty.Count, bool sortAscending = false, bool expanded = false)
        {
            var tagSearchOptions = await GetTagSearchOptions(tagSearchType, null, galleryId, top, sortBy, sortAscending, expanded);

            return GetTagTree(tagSearchOptions).ToJson();
        }

        #endregion

        #region Functions

        /// <summary>
        /// Generate a collection of all tag values that exist associated with the specified
        /// <paramref name="galleryItems" /> having the specified <paramref name="tagName" />.
        /// Individual tag values will be repeated when they belong to multiple gallery items.
        /// </summary>
        /// <param name="galleryItems">The gallery items.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <returns>Returns a collection of strings.</returns>
        /// <exception cref="InvalidAlbumException">Thrown when the requested album does not exist.</exception>
        /// <exception cref="InvalidMediaObjectException">Thrown when the requested media object does not exist.</exception>
        private IEnumerable<string> GetTagListForGalleryItems(IEnumerable<Entity.GalleryItem> galleryItems, MetadataItemName tagName)
        {
            var tagList = new List<string>();
            foreach (var metas in galleryItems.Select(GetGalleryObjectMetadataItemCollection))
            {
                tagList.AddRange(GetTagList(metas, tagName));
            }
            return tagList;
        }

        /// <summary>
        /// Gets the collection of tag values having the specified <paramref name="tagName" />.
        /// </summary>
        /// <param name="metas">The metadata items.</param>
        /// <param name="tagName">Name of the tag.</param>
        /// <returns>Returns a collection of strings.</returns>
        private IEnumerable<string> GetTagList(IGalleryObjectMetadataItemCollection metas, MetadataItemName tagName)
        {
            IGalleryObjectMetadataItem mdTag;
            if (metas.TryGetMetadataItem(tagName, out mdTag))
            {
                return mdTag.Value.ToListFromCommaDelimited();
            }
            else
                return new string[] { };
        }

        /// <overloads>
        /// Gets the metadata collection for the specified criteria. Guaranteed to not return null.
        /// </overloads>
        /// <summary>
        /// Gets the metadata collection for the specified <paramref name="galleryItem" />.
        /// </summary>
        /// <param name="galleryItem">The gallery item representing either an album or a media object.</param>
        /// <returns>Returns an instance of <see cref="IGalleryObjectMetadataItemCollection" />.</returns>
        /// <exception cref="InvalidAlbumException">Thrown when the requested album does not exist.</exception>
        /// <exception cref="InvalidMediaObjectException">Thrown when the requested media object does not exist.</exception>
        private IGalleryObjectMetadataItemCollection GetGalleryObjectMetadataItemCollection(Entity.GalleryItem galleryItem)
        {
            return GetGalleryObjectMetadataItemCollection(galleryItem.Id, (GalleryObjectType)galleryItem.ItemType);
        }

        /// <summary>
        /// Gets the metadata collection for the specified <paramref name="galleryObjectId" /> and
        /// <paramref name="goType" />.
        /// </summary>
        /// <param name="galleryObjectId">The ID for either an album or a media object.</param>
        /// <param name="goType">The type of gallery object.</param>
        /// <returns>Returns an instance of <see cref="IGalleryObjectMetadataItemCollection" />.</returns>
        /// <exception cref="InvalidAlbumException">Thrown when the requested album does not exist.</exception>
        /// <exception cref="InvalidMediaObjectException">Thrown when the requested media object does not exist.</exception>
        private IGalleryObjectMetadataItemCollection GetGalleryObjectMetadataItemCollection(int galleryObjectId, GalleryObjectType goType)
        {
            if (goType == GalleryObjectType.Album)
                return Factory.LoadAlbumInstance(galleryObjectId).MetadataItems;
            else
                return Factory.LoadMediaObjectInstance(galleryObjectId).MetadataItems;
        }

        /// <summary>
        /// Process the <paramref name="tags" /> and return a comma-delimited string containing the
        /// tag values and their counts. Ex: "Animal (3), Dog (2), Cat (1)"
        /// </summary>
        /// <param name="tags">The tags to process.</param>
        /// <returns>Returns a string.</returns>
        private string GetTagsWithCount(IEnumerable<string> tags)
        {
            // Group the tags by their value and build up a unique list containing the value and their
            // count in parenthesis.
            var tagsGrouped = new List<String>();

            foreach (var item in tags.GroupBy(w => w).OrderByDescending(w => w.Count()))
            {
                tagsGrouped.Add(String.Format(CultureInfo.InvariantCulture, "{0} ({1})", item.Key, item.Count()));
            }

            return String.Join(", ", tagsGrouped);
        }

        private Task<MetaItem[]> GetMetaItems(Entity.GalleryItem lastGi)
        {
            if (lastGi.ItemType == (int)GalleryObjectType.Album)
                return _albumController.GetMetaItemsForAlbum(lastGi.Id);
            else
                return _galleryObjectController.GetMetaItemsForMediaObject(lastGi.Id);
        }

        private Entity.MetaItem GetMetaItemForTag(Entity.MetaItem[] meta, MetadataItemName tagName, Entity.GalleryItem galleryItem)
        {
            var tagMi = meta.FirstOrDefault(m => m.MTypeId == (int)tagName);
            if (tagMi != null)
            {
                return tagMi;
            }
            else
            {
                // Last item doesn't have a tag. Create one. This code path should be pretty rare.
                int galleryId;
                if (galleryItem.IsAlbum)
                    galleryId = _albumController.LoadAlbumInstance(galleryItem.Id).GalleryId;
                else
                    galleryId = Factory.LoadMediaObjectInstance(galleryItem.Id).GalleryId;

                var metaDef = Factory.LoadGallerySetting(galleryId).MetadataDisplaySettings.Find(tagName);

                // The HTML editor requires the trial version or Home & Nonprofit or higher.
                var editMode = (metaDef.UserEditMode == PropertyEditorMode.TinyMCEHtmlEditor && AppSetting.Instance.License.LicenseType < LicenseLevel.HomeNonprofit ? PropertyEditorMode.PlainTextEditor : metaDef.UserEditMode);

                tagMi = new Entity.MetaItem
                {
                    Id = int.MinValue,
                    MediaId = galleryItem.Id,
                    GTypeId = galleryItem.ItemType,
                    MTypeId = (int)tagName,
                    Desc = tagName.ToString(),
                    Value = String.Empty,
                    //IsEditable = metaDef.IsEditable,
                    EditMode = editMode
                };

                Array.Resize(ref meta, meta.Count() + 1);
                meta[meta.Length - 1] = tagMi;

                return tagMi;
            }
        }

        /// <summary>
        /// Adds the tag to the specified gallery items. This method is intended only for tag-style
        /// metadata items, such as descriptive tags and people. It is assumed the metadata item in
        /// the data store is a comma-separated list of tags, and the value passed in to this method is to 
        /// be added to it. No action is taken on a gallery object if the tag already exists or the
        /// specified gallery object does not exist.
        /// </summary>
        /// <param name="galleryItemTag">An instance of <see cref="Entity.GalleryItemMeta" /> that defines
        /// the tag value to be added and the gallery items it is to be added to.</param>
        /// <exception cref="GallerySecurityException">Thrown when the current user does not have
        /// permission to edit one or more of the specified gallery items.</exception>
        /// <exception cref="WebException">Thrown when the metadata instance is not a tag-style item.</exception>
        private async Task AddTag(Entity.GalleryItemMeta galleryItemTag)
        {
            var metaName = (MetadataItemName)galleryItemTag.MetaItem.MTypeId;

            if (metaName != MetadataItemName.Tags && metaName != MetadataItemName.People)
                throw new WebException(string.Format("The AddTag function is designed to persist tag-style metadata items. The item that was passed ({0}) does not qualify.", metaName.ToString()));

            foreach (var galleryItem in galleryItemTag.GalleryItems)
            {
                IGalleryObject galleryObject = await GetGalleryObjectAndVerifyEditPermission(galleryItem);
                if (galleryObject == null)
                    continue;

                IGalleryObjectMetadataItem md;
                if (galleryObject.MetadataItems.TryGetMetadataItem((MetadataItemName)galleryItemTag.MetaItem.MTypeId, out md))
                {
                    // Split tag into array, add it if it's not already there, and save
                    var tags = md.Value.Split(new string[] { ", ", "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (!tags.Contains(galleryItemTag.MetaItem.Value, StringComparer.OrdinalIgnoreCase))
                    {
                        tags.Add(Utils.CleanHtmlTags(galleryItemTag.MetaItem.Value, galleryObject.GalleryId));

                        md.Value = String.Join(", ", tags);

                        Factory.SaveGalleryObjectMetadataItem(md, _userController.UserName);

                        CacheController.PurgeCache(galleryObject);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the gallery items with the specified metadata value. Values are scrubbed of HTML and script per the gallery
        /// configuration. The property <see cref="Entity.GalleryItemMeta.ActionResult" /> of <paramref name="galleryItemMeta" /> 
        /// is assigned when a validation error occurs, but remains null for a successful operation. If we updated the property 
        /// for assets in an album that is sorted on that property, the album is resorted.
        /// </summary>
        /// <param name="galleryItemMeta">An object containing the metadata instance to use as the source
        /// and the gallery items to be updated.</param>
        /// <exception cref="GallerySecurityException">Thrown when the current user does not have
        /// permission to edit one or more of the specified gallery items.</exception>
        /// <exception cref="WebException">Thrown when the metadata instance is a tag-style item.</exception>
        private async Task PersistGalleryItemMeta(Entity.GalleryItemMeta galleryItemMeta)
        {
            var metaName = (MetadataItemName)galleryItemMeta.MetaItem.MTypeId;

            if (metaName == MetadataItemName.Tags || metaName == MetadataItemName.People)
                throw new WebException("The PersistGalleryItemMeta function is not designed to persist tag-style metadata items.");

            var metaValueHasBeenCleaned = false;
            var affectedAlbums = new Dictionary<int, IAlbum>();

            foreach (var galleryItem in galleryItemMeta.GalleryItems)
            {
                IGalleryObject galleryObject = await GetGalleryObjectAndVerifyEditPermission(galleryItem);
                if (galleryObject == null)
                    continue;

                if (galleryItemMeta.MetaItem.MTypeId == (int)MetadataItemName.Title && String.IsNullOrWhiteSpace(galleryItemMeta.MetaItem.Value) && galleryItemMeta.GalleryItems.Any(g => g.IsAlbum))
                {
                    galleryItemMeta.ActionResult = new ActionResult()
                    {
                        Status = ActionResultStatus.Error.ToString(),
                        Title = "Cannot save changes",
                        Message = "An album title cannot be set to a blank string."
                    };
                    return;
                }

                if (!metaValueHasBeenCleaned)
                {
                    // Clean just once regardless of how many gallery items we are updating. We expect all gallery items are in the same gallery.
                    galleryItemMeta.MetaItem.Value = Utils.CleanHtmlTags(galleryItemMeta.MetaItem.Value, galleryObject.GalleryId);
                    metaValueHasBeenCleaned = true;
                }

                IGalleryObjectMetadataItem metaItem;
                if (galleryObject.MetadataItems.TryGetMetadataItem(metaName, out metaItem) && metaItem.IsEditable)
                {
                    metaItem.Value = galleryItemMeta.MetaItem.Value;
                    Factory.SaveGalleryObjectMetadataItem(metaItem, _userController.UserName);

                    CacheController.PurgeCache(galleryObject);

                    if (!affectedAlbums.ContainsKey(galleryObject.Parent.Id) && (galleryObject.Parent is IAlbum))
                    {
                        affectedAlbums.Add(galleryObject.Parent.Id, (IAlbum)galleryObject.Parent);
                    }
                }
                else
                {
                    // Get a writeable instance of the gallery object and create new metadata instance.
                    if (galleryItem.ItemType == (int)GalleryObjectType.Album)
                        galleryObject = Factory.LoadAlbumInstance(new AlbumLoadOptions(galleryItem.Id) { IsWritable = true });
                    else
                        galleryObject = Factory.LoadMediaObjectInstance(new MediaLoadOptions(galleryItem.Id) { IsWritable = true, Album = (IAlbum)galleryObject.Parent });


                    // Add the new metadata item.
                    var metaDef = Factory.LoadGallerySetting(galleryObject.GalleryId).MetadataDisplaySettings.Find(metaName);

                    if (metaDef.IsEditable && galleryObject.MetadataDefinitionApplies(metaDef))
                    {
                        var metaItems = Factory.CreateMetadataCollection();
                        metaItems.Add(Factory.CreateMetadataItem(int.MinValue, galleryObject, null, galleryItemMeta.MetaItem.Value, true, metaDef));
                        galleryObject.AddMeta(metaItems);

                        await _galleryObjectController.SaveGalleryObject(galleryObject);

                        if (!affectedAlbums.ContainsKey(galleryObject.Parent.Id))
                        {
                            affectedAlbums.Add(galleryObject.Parent.Id, (IAlbum)galleryObject.Parent);
                        }
                    }
                }
            }

            // If we updated the property for assets in an album that is sorted on that property, re-sort the album.
            foreach (var kvp in affectedAlbums)
            {
                if (kvp.Value.SortByMetaName == (MetadataItemName)galleryItemMeta.MetaItem.MTypeId)
                {
                    var affectedAlbum = Factory.LoadAlbumInstance(new AlbumLoadOptions(kvp.Value.Id) { IsWritable = true });
                    affectedAlbum.SortAsync(true, _userController.UserName);
                }
            }
        }

        /// <summary>
        /// Gets the gallery object for the specified <paramref name="galleryItem" /> and verifies current
        /// user has edit permission, throwing a <see cref="GallerySecurityException" /> if needed. Returns
        /// null if no media object or album having the requested ID exists.
        /// </summary>
        /// <param name="galleryItem">The gallery item.</param>
        /// <returns>An instance of <see cref="IGalleryObject" /> corresponding to <paramref name="galleryItem" />.</returns>
        /// <exception cref="GallerySecurityException">Thrown when the current user does not have
        /// permission to edit the specified gallery items.</exception>
        private async Task<IGalleryObject> GetGalleryObjectAndVerifyEditPermission(Entity.GalleryItem galleryItem)
        {
            IGalleryObject galleryObject = null;
            try
            {
                if (galleryItem.ItemType == (int)GalleryObjectType.Album)
                {
                    galleryObject = Factory.LoadAlbumInstance(galleryItem.Id);
                    SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.EditAlbum, await _userController.GetGalleryServerRolesForUser(), galleryObject.Id, galleryObject.GalleryId, _userController.IsAuthenticated, galleryObject.IsPrivate, ((IAlbum)galleryObject).IsVirtualAlbum);
                }
                else
                {
                    galleryObject = Factory.LoadMediaObjectInstance(galleryItem.Id);
                    SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.EditMediaObject, await _userController.GetGalleryServerRolesForUser(), galleryObject.Parent.Id, galleryObject.GalleryId, _userController.IsAuthenticated, galleryObject.Parent.IsPrivate, ((IAlbum)galleryObject.Parent).IsVirtualAlbum);
                }
            }
            catch (InvalidAlbumException ex)
            {
                AppEventController.LogError(ex);
            }
            catch (InvalidMediaObjectException ex)
            {
                AppEventController.LogError(ex);
            }

            return galleryObject;
        }

        /// <summary>
        /// Gets the read-only gallery object for the specified <paramref name="galleryItem" /> and verifies current
        /// user has the ability to edit its rating, throwing a <see cref="GallerySecurityException" /> if needed. Returns
        /// null if no media object or album having the requested ID exists.
        /// </summary>
        /// <param name="galleryItem">The gallery item.</param>
        /// <returns>An instance of <see cref="IGalleryObject" /> corresponding to <paramref name="galleryItem" />.</returns>
        /// <exception cref="GallerySecurityException">Thrown when the current user does not have
        /// permission to edit the specified gallery items.</exception>
        /// <remarks>Editing a rating works a little different than other metadata: Anonymous users are allowed
        /// to apply a rating as long as <see cref="IGallerySettings.AllowAnonymousRating" /> is <c>true</c> and all
        /// logged on users are allowed to rate an item.</remarks>
        private async Task<IGalleryObject> GetGalleryObjectAndVerifyEditRatingPermission(Entity.GalleryItem galleryItem)
        {
            IGalleryObject galleryObject = null;
            try
            {
                if (galleryItem.ItemType == (int)GalleryObjectType.Album)
                {
                    galleryObject = Factory.LoadAlbumInstance(galleryItem.Id);
                    SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.ViewAlbumOrMediaObject, await _userController.GetGalleryServerRolesForUser(), galleryObject.Id, galleryObject.GalleryId, _userController.IsAuthenticated, galleryObject.IsPrivate, ((IAlbum)galleryObject).IsVirtualAlbum);
                }
                else
                {
                    galleryObject = Factory.LoadMediaObjectInstance(galleryItem.Id);
                    SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.ViewAlbumOrMediaObject, await _userController.GetGalleryServerRolesForUser(), galleryObject.Parent.Id, galleryObject.GalleryId, _userController.IsAuthenticated, galleryObject.Parent.IsPrivate, ((IAlbum)galleryObject.Parent).IsVirtualAlbum);
                }

                if (!_userController.IsAuthenticated && !Factory.LoadGallerySetting(galleryObject.GalleryId).AllowAnonymousRating)
                {
                    // We have an anonymous user attempting to rate an item, but the AllowAnonymousRating setting is false.
                    galleryObject = null;
                    throw new GallerySecurityException(String.Format("An anonymous user is attempting to rate a gallery object ({0} ID {1}), but the gallery is configured to not allow ratings by anonymous users. The request is denied.", (GalleryObjectType)galleryItem.ItemType, galleryObject.Id));
                }
            }
            catch (InvalidAlbumException ex)
            {
                AppEventController.LogError(ex);
            }
            catch (InvalidMediaObjectException ex)
            {
                AppEventController.LogError(ex);
            }

            return galleryObject;
        }

        private void StartWriteMetaItem(IMetadataDefinition metaDef, IGalleryObject galleryObject, string userName)
        {
            try
            {
                AppEventController.LogEvent(String.Format(CultureInfo.CurrentCulture, "INFO: Starting to batch write metadata item '{0}' for all assets in gallery {1}.", metaDef.MetadataItem, galleryObject.GalleryId), galleryObject.GalleryId);

                WriteMetaItem(metaDef, galleryObject, userName);

                AppEventController.LogEvent(String.Format(CultureInfo.CurrentCulture, "INFO: Successfully finished batch writing metadata item '{0}' for all assets in gallery {1}.", metaDef.MetadataItem, galleryObject.GalleryId), galleryObject.GalleryId);
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex, galleryObject.GalleryId);
                AppEventController.LogEvent(String.Format(CultureInfo.CurrentCulture, "CANCELED: The batch writing of metadata item '{0}' for all assets in gallery {1} has been canceled due to the previously logged error.", metaDef.MetadataItem, galleryObject.GalleryId), galleryObject.GalleryId);
                throw;
            }
        }

        private void WriteMetaItem(IMetadataDefinition metaDef, IGalleryObject galleryObject, string userName)
        {
            IGalleryObjectMetadataItem metaItem;
            if (galleryObject.MetadataItems.TryGetMetadataItem(metaDef.MetadataItem, out metaItem))
            {
                metaItem.PersistToFile = true;
                Factory.SaveGalleryObjectMetadataItem(metaItem, userName);

                CacheController.PurgeCache(galleryObject);
            }

            if (galleryObject.GalleryObjectType == GalleryObjectType.Album)
            {
                foreach (var go in galleryObject.GetChildGalleryObjects())
                {
                    WriteMetaItem(metaDef, go, userName);
                }
            }
        }

        private void StartRebuildMetaItem(IMetadataDefinition metaDef, IGalleryObject galleryObject, string userName)
        {
            try
            {
                AppEventController.LogEvent(String.Format(CultureInfo.CurrentCulture, "INFO: Starting to re-extract metadata item '{0}' for all assets in gallery {1}.", metaDef.MetadataItem, galleryObject.GalleryId), galleryObject.GalleryId);

                RebuildMetaItem(metaDef, galleryObject, userName);

                if (metaDef.MetadataItem == MetadataItemName.Tags || metaDef.MetadataItem == MetadataItemName.People)
                {
                    Factory.DeleteUnusedTags();
                }

                AppEventController.LogEvent(String.Format(CultureInfo.CurrentCulture, "INFO: Successfully finished re-extracting metadata item '{0}' for all assets in gallery {1}.", metaDef.MetadataItem, galleryObject.GalleryId), galleryObject.GalleryId);
            }
            catch (Exception ex)
            {
                AppEventController.LogError(ex, galleryObject.GalleryId);
                AppEventController.LogEvent(String.Format(CultureInfo.CurrentCulture, "CANCELED: The re-extracting of metadata item '{0}' for all assets in gallery {1} has been canceled due to the previously logged error.", metaDef.MetadataItem, galleryObject.GalleryId), galleryObject.GalleryId);
                throw;
            }
        }

        private void RebuildMetaItem(IMetadataDefinition metaDef, IGalleryObject galleryObject, string userName)
        {
            galleryObject.ExtractMetadata(metaDef);

            IGalleryObjectMetadataItem metaItem;
            if (galleryObject.MetadataItems.TryGetMetadataItem(metaDef.MetadataItem, out metaItem))
            {
                metaItem.PersistToFile = false;
                Factory.SaveGalleryObjectMetadataItem(metaItem, userName);

                CacheController.PurgeCache(galleryObject);
            }

            if (galleryObject.GalleryObjectType == GalleryObjectType.Album)
            {
                foreach (var go in galleryObject.GetChildGalleryObjects())
                {
                    RebuildMetaItem(metaDef, go, userName);
                }
            }
        }

        /// <summary>
        /// Apply a user's rating to the gallery items specified in <paramref name="galleryItemMeta" /> and persist to the
        /// data store. A record of the user's rating is stored in their profile. If rating is not a number, then no action
        /// is taken. If rating is less than 0 or greater than 5, it is assigned to 0 or 5 so that the value is guaranteed
        /// to be between those values.
        /// </summary>
        /// <param name="galleryItemMeta">An instance containing the rating metadata item and the gallery objects to which
        /// it applies.</param>
        /// <exception cref="WebException">Thrown when the metadata item is not <see cref="MetadataItemName.Rating" />.</exception>
        private async Task PersistRating(Entity.GalleryItemMeta galleryItemMeta)
        {
            var metaName = (MetadataItemName)galleryItemMeta.MetaItem.MTypeId;

            if (metaName != MetadataItemName.Rating)
                throw new WebException(string.Format("The PersistRating function is designed to store 'Rating' metadata items, but {0} was passed.", metaName));

            // If rating is not a number, then return without doing anything; otherwise verify rating is between 0 and 5.
            const byte minRating = 0;
            const byte maxRating = 5;
            float rating;
            if (Single.TryParse(galleryItemMeta.MetaItem.Value, out rating))
            {
                if (rating < minRating)
                    galleryItemMeta.MetaItem.Value = minRating.ToString(CultureInfo.InvariantCulture);

                if (rating > maxRating)
                    galleryItemMeta.MetaItem.Value = maxRating.ToString(CultureInfo.InvariantCulture);
            }
            else
                return; // Can't parse rating, so return without doing anything

            foreach (var galleryItem in galleryItemMeta.GalleryItems)
            {
                IGalleryObject galleryObject = await GetGalleryObjectAndVerifyEditRatingPermission(galleryItem);
                if (galleryObject == null)
                    continue;

                IGalleryObjectMetadataItem ratingItem;
                if (galleryObject.MetadataItems.TryGetMetadataItem(metaName, out ratingItem))
                {
                    if (ratingItem.IsEditable)
                    {
                        // We have an existing rating item. Incorporate the user's rating into the average and persist.
                        ratingItem.Value = CalculateAvgRating(galleryObject, ratingItem, galleryItemMeta.MetaItem.Value);

                        Factory.SaveGalleryObjectMetadataItem(ratingItem, _userController.UserName);

                        PersistRatingInUserProfile(galleryObject.Id, galleryItemMeta.MetaItem.Value);
                    }
                }
                else
                {
                    // No rating item found for this gallery item. Create one (if business rules allow).
                    var metaDefRating = Factory.LoadGallerySetting(galleryObject.GalleryId).MetadataDisplaySettings.Find(metaName);

                    if (metaDefRating.IsEditable && galleryObject.MetadataDefinitionApplies(metaDefRating))
                    {
                        var metaItems = Factory.CreateMetadataCollection();
                        var newRatingItem = Factory.CreateMetadataItem(int.MinValue, galleryObject, null, galleryItemMeta.MetaItem.Value, true, metaDefRating);
                        metaItems.Add(newRatingItem);
                        galleryObject.AddMeta(metaItems);

                        var ratingCountItem = GetRatingCountMetaItem(galleryObject);
                        ratingCountItem.Value = "1"; // This is the first rating

                        Factory.SaveGalleryObjectMetadataItem(newRatingItem, _userController.UserName);
                        Factory.SaveGalleryObjectMetadataItem(ratingCountItem, _userController.UserName);

                        PersistRatingInUserProfile(galleryItem.Id, galleryItemMeta.MetaItem.Value);
                    }
                }

                CacheController.PurgeCache(galleryObject);
            }
        }

        /// <summary>
        /// Incorporate the new <paramref name="userRatingStr" /> into the current <paramref name="ratingItem" />
        /// belonging to the <paramref name="galleryObject" />. Automatically increments and saves the rating count
        /// meta item. Detects when a user has previously rated the item and reverses the effects of the previous 
        /// rating before applying the new one. Returns a <see cref="System.Single" /> converted to a string to 4 
        /// decimal places (e.g. "2.4653").
        /// </summary>
        /// <param name="galleryObject">The gallery object being rated.</param>
        /// <param name="ratingItem">The rating metadata item.</param>
        /// <param name="userRatingStr">The user rating to be applied to the gallery object rating.</param>
        /// <returns>Returns a <see cref="System.String" /> representing the new rating.</returns>
        private string CalculateAvgRating(IGalleryObject galleryObject, IGalleryObjectMetadataItem ratingItem, string userRatingStr)
        {
            var ratingCountItem = GetRatingCountMetaItem(galleryObject);
            int ratingCount;
            Int32.TryParse(ratingCountItem.Value, out ratingCount);

            float currentAvgRating, userRating;
            Single.TryParse(ratingItem.Value, out currentAvgRating);
            Single.TryParse(userRatingStr, out userRating);

            var moProfile = ProfileController.GetProfile(_userController.UserName).MediaObjectProfiles.Find(ratingItem.GalleryObject.Id);
            if (moProfile != null)
            {
                // User has previously rated this item. Reverse the influence that rating had on the item's average rating.
                currentAvgRating = RemoveUsersPreviousRating(ratingItem.Value, ratingCount, moProfile.Rating);

                // Subtract the user's previous rating from the total rating count while ensuring the # >= 0.
                ratingCount = Math.Max(ratingCount - 1, 0);
            }

            // Increment the rating count and persist.
            ratingCount++;
            ratingCountItem.Value = ratingCount.ToString(CultureInfo.InvariantCulture);

            Factory.SaveGalleryObjectMetadataItem(ratingCountItem, _userController.UserName);

            // Calculate the new rating.
            float newAvgRating = ((currentAvgRating * (ratingCount - 1)) + userRating) / (ratingCount);

            return newAvgRating.ToString("F4", CultureInfo.InvariantCulture); // Store rating to 4 decimal places
        }

        /// <summary>
        /// Reverse the influence a user's rating had on the media object's average rating. The new average rating
        /// is returned. Returns zero if <paramref name="currentAvgRatingStr" /> or <paramref name="userPrevRatingStr" />
        /// can't be converted to a <see cref="System.Single" /> or if <paramref name="ratingCount" /> is less than
        /// or equal to one.
        /// </summary>
        /// <param name="currentAvgRatingStr">The current average rating for the media object as a string (e.g. "2.8374").</param>
        /// <param name="ratingCount">The number of times tje media object has been rated.</param>
        /// <param name="userPrevRatingStr">The user rating whose effect must be removed from the average rating (e.g. "2.5").</param>
        /// <returns><see cref="System.Single" />.</returns>
        private float RemoveUsersPreviousRating(string currentAvgRatingStr, int ratingCount, string userPrevRatingStr)
        {
            if (ratingCount <= 1)
                return 0f;

            float currentAvgRating, userRating;
            if (!Single.TryParse(userPrevRatingStr, out userRating))
                return 0f;

            if (!Single.TryParse(currentAvgRatingStr, out currentAvgRating))
                return 0f;

            return ((currentAvgRating * ratingCount) - userRating) / (ratingCount - 1);
        }

        /// <summary>
        /// Gets the rating count meta item for the <paramref name="galleryObject" />, creating one - and persisting it
        /// to the data store - if necessary.
        /// </summary>
        /// <param name="galleryObject">The gallery object.</param>
        /// <returns><see cref="IGalleryObjectMetadataItem" />.</returns>
        private IGalleryObjectMetadataItem GetRatingCountMetaItem(IGalleryObject galleryObject)
        {
            IGalleryObjectMetadataItem metaItem;
            if (galleryObject.MetadataItems.TryGetMetadataItem(MetadataItemName.RatingCount, out metaItem))
            {
                return metaItem;
            }
            else
            {
                return CreateRatingCountMetaItem(galleryObject);
            }
        }

        /// <summary>
        /// Create a rating count meta item for the <paramref name="galleryObject" /> and persist it to the data store.
        /// </summary>
        /// <param name="galleryObject">The gallery object.</param>
        /// <returns>An instance of <see cref="IGalleryObjectMetadataItem" />.</returns>
        private IGalleryObjectMetadataItem CreateRatingCountMetaItem(IGalleryObject galleryObject)
        {
            // Create the rating count item, add it to the gallery object, and save.
            var metaDef = Factory.LoadGallerySetting(galleryObject.GalleryId).MetadataDisplaySettings.Find(MetadataItemName.RatingCount);
            var metaItems = Factory.CreateMetadataCollection();
            var ratingCountItem = Factory.CreateMetadataItem(int.MinValue, galleryObject, null, "0", true, metaDef);
            metaItems.Add(ratingCountItem);
            galleryObject.AddMeta(metaItems);

            Factory.SaveGalleryObjectMetadataItem(ratingCountItem, _userController.UserName);

            return ratingCountItem;
        }

        /// <summary>
        /// Persists the user's rating in their user profile.
        /// </summary>
        /// <param name="mediaObjectId">The media object ID of the item being rated.</param>
        /// <param name="userRating">The user rating (e.g. "2.8374").</param>
        private void PersistRatingInUserProfile(int mediaObjectId, string userRating)
        {
            var profile = ProfileController.GetProfile(_userController.UserName);

            var moProfile = profile.MediaObjectProfiles.Find(mediaObjectId);

            if (moProfile == null)
            {
                profile.MediaObjectProfiles.Add(new MediaObjectProfile(mediaObjectId, userRating));
            }
            else
            {
                moProfile.Rating = userRating;
            }

            ProfileController.SaveProfile(profile);
        }

        /// <summary>
        /// Gets a tree representing the tags used in a gallery. The tree has a root node that serves as the tag container.
        /// It contains a flat list of child nodes for the tags.
        /// </summary>
        /// <param name="tagSearchOptions">The options that specify what kind of tags to return and how they should be
        /// calculated and displayed.</param>
        /// <returns>Returns an instance of <see cref="Entity.TreeView" />. Guaranteed to not return null.</returns>
        private Entity.TreeView GetTagTree(TagSearchOptions tagSearchOptions)
        {
            var tags = GetTags(tagSearchOptions);
            var id = 0;
            var tv = new Entity.TreeView();
            //var baseUrl = Utils.GetCurrentPageUrl();
            var qsParm = GetTagTreeNavUrlQsParm(tagSearchOptions.SearchType);

            var rootNode = new Entity.TreeNode
            {
                Text = GetTagTreeRootNodeText(tagSearchOptions.SearchType),
                //ToolTip = "Tags in gallery",
                Id = String.Concat("tv_tags_", id++),
                DataId = "root",
                Expanded = tagSearchOptions.TagTreeIsExpanded,
            };

            rootNode.AddCssClass("jstree-root-node");

            tv.Nodes.Add(rootNode);

            foreach (var tag in tags)
            {
                rootNode.Nodes.Add(new Entity.TreeNode
                {
                    Text = String.Format(CultureInfo.InvariantCulture, "{0} ({1})", tag.Value, tag.Count),
                    ToolTip = $"Click to view all items tagged with {tag.Value}",
                    Id = String.Concat("tv_tags_", id++),
                    DataId = tag.Value,
                    //NavigateUrl = Utils.AddQueryStringParameter(baseUrl, String.Concat(qsParm, "=", Utils.UrlEncode(tag.Value)))
                });
            }

            return tv;
        }

        private string GetTagTreeRootNodeText(TagSearchType searchType)
        {
            switch (searchType)
            {
                case TagSearchType.AllTagsInGallery:
                case TagSearchType.TagsUserCanView:
                    return "POPULAR TAGS";

                case TagSearchType.AllPeopleInGallery:
                case TagSearchType.PeopleUserCanView:
                    return "PEOPLE";

                default:
                    throw new ArgumentException(String.Format("This function is not expecting TagSearchType={0}", searchType));
            }
        }

        private string GetTagTreeNavUrlQsParm(TagSearchType searchType)
        {
            switch (searchType)
            {
                case TagSearchType.AllTagsInGallery:
                case TagSearchType.TagsUserCanView:
                    return "tag";

                case TagSearchType.AllPeopleInGallery:
                case TagSearchType.PeopleUserCanView:
                    return "people";

                default:
                    throw new ArgumentException(String.Format("This function is not expecting TagSearchType={0}", searchType));
            }
        }

        private async Task<TagSearchOptions> GetTagSearchOptions(TagSearchType searchType, string searchTerm, int galleryId, int numTagsToRetrieve = int.MaxValue, TagSearchOptions.TagProperty sortProperty = TagSearchOptions.TagProperty.NotSpecified, bool sortAscending = true, bool expanded = false)
        {
            return new TagSearchOptions
            {
                GalleryId = galleryId,
                SearchType = searchType,
                SearchTerm = searchTerm,
                IsUserAuthenticated = _userController.IsAuthenticated,
                Roles = await _userController.GetGalleryServerRolesForUser(),
                NumTagsToRetrieve = numTagsToRetrieve,
                SortProperty = sortProperty,
                SortAscending = sortAscending,
                TagTreeIsExpanded = expanded
            };
        }

        #endregion
    }
}