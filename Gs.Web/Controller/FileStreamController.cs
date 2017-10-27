using System;
using GalleryServer.Business;
using System.IO;
using System.Threading.Tasks;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Web.Controller
{
    /// <summary>
    /// Contains functionality for returning <see cref="FileStream" /> instances associated with media assets.
    /// </summary>
    public class FileStreamController
    {
        private readonly UserController _userController;
        private DisplayObjectType _displayType;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStreamController"/> class.
        /// </summary>
        /// <param name="userController">The user controller.</param>
        public FileStreamController(UserController userController)
        {
            _userController = userController;
        }

        /// <summary>
        /// Gets or set the full mime type. E.g. image/jpeg, video/mp4
        /// </summary>
        /// <value>The full mime type.</value>
        public string ContentType => DisplayAsset.MimeType.FullType;

        /// <summary>
        /// Gets or sets the media asset.
        /// </summary>
        /// <value>The media asset.</value>
        private IGalleryObject MediaAsset { get; set; }

        /// <summary>
        /// Gets the display asset.
        /// </summary>
        /// <value>The display asset.</value>
        /// <exception cref="InvalidOperationException"></exception>
        private IDisplayObject DisplayAsset
        {
            get
            {
                switch (_displayType)
                {
                    case DisplayObjectType.Thumbnail:
                        return MediaAsset.Thumbnail;
                    case DisplayObjectType.Optimized:
                        return MediaAsset.Optimized;
                    case DisplayObjectType.Original:
                        return MediaAsset.Original;
                    default:
                        throw new InvalidOperationException($"FileStreamController.DisplayAsset is not designed to handle the DisplayObjectType enumeration value {_displayType}.");
                }
            }
        }

        /// <summary>
        /// Assigns the <paramref name="mediaId" /> and <paramref name="displayType" /> for this instance. This should be called before invoking any other methods or properties.
        /// </summary>
        /// <param name="mediaId">The media ID.</param>
        /// <param name="displayType">The display type.</param>
        public void SetMedia(int mediaId, DisplayObjectType displayType)
        {
            _displayType = displayType;

            MediaAsset = Factory.LoadMediaObjectInstance(mediaId);
        }

        /// <summary>
        /// Gets the media file as a stream.
        /// </summary>
        /// <returns>An instance of <see cref="FileStream" />.</returns>
        /// <exception cref="Events.CustomExceptions.GallerySecurityException">Thrown when user is not authorized.</exception>
        public async Task<FileStream> GetStream()
        {
            if (DisplayAsset == null)
                throw new InvalidOperationException("FileStreamController.SetMedia() must be called before invoking FileStreamController.GetStream().");

            SecurityManager.ThrowIfUserNotAuthorized(SecurityActions.ViewAlbumOrMediaObject, await _userController.GetGalleryServerRolesForUser(), MediaAsset.Parent.Id, MediaAsset.GalleryId, _userController.IsAuthenticated, MediaAsset.Parent.IsPrivate, ((IAlbum)MediaAsset.Parent).IsVirtualAlbum);

            return new FileStream(DisplayAsset.FileNamePhysicalPath, FileMode.Open);
        }
    }
}
