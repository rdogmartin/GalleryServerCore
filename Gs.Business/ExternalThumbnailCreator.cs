using GalleryServer.Business.Interfaces;
using System;
using System.Globalization;
using System.IO;

namespace GalleryServer.Business
{
    /// <summary>
    /// Provides functionality for creating and saving the thumbnail image files associated with <see cref="ExternalMediaObject" /> gallery objects.
    /// </summary>
    public class ExternalThumbnailCreator : DisplayObjectCreator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExternalThumbnailCreator"/> class.
        /// </summary>
        /// <param name="galleryObject">The gallery object.</param>
        public ExternalThumbnailCreator(GalleryObject galleryObject)
        {
            GalleryObject = galleryObject;
        }

        /// <summary>
        /// Generate the thumbnail image for this display object and save it to the file system. The routine may decide that
        /// a file does not need to be generated, usually because it already exists. However, it will always be
        /// created if the relevant flag is set on the parent <see cref="IGalleryObject" />. (Example: If
        /// <see cref="IGalleryObject.RegenerateThumbnailOnSave" /> = true, the thumbnail file will always be created.) No data is
        /// persisted to the data store.
        /// </summary>
        public override void GenerateAndSaveFile()
        {
            // If necessary, generate and save the thumbnail version of the original image.
            if (!(IsThumbnailImageRequired()))
            {
                return; // No thumbnail image required.
            }

            IGallerySettings gallerySetting = Factory.LoadGallerySetting(GalleryObject.GalleryId);

            // Determine file name and path of the thumbnail image. If a file name has already been previously
            // calculated for this media object, re-use it. Otherwise generate a unique name.
            var newFilename = GalleryObject.Thumbnail.FileName;
            var newFilePath = GalleryObject.Thumbnail.FileNamePhysicalPath;

            if (String.IsNullOrEmpty(newFilePath))
            {
                var thumbnailPath = HelperFunctions.MapAlbumDirectoryStructureToAlternateDirectory(this.GalleryObject.Parent.FullPhysicalPath, gallerySetting.FullThumbnailPath, gallerySetting.FullMediaObjectPath);
                newFilename = HelperFunctions.ValidateFileName(thumbnailPath, GenerateNewFilename(gallerySetting.ThumbnailFileNamePrefix));
                newFilePath = Path.Combine(thumbnailPath, newFilename);
            }

            // Get reference to the bitmap from which the thumbnail image will be generated.
            var sourceFilePath = GetGenericThumbnailFilePath(GalleryObject.MimeType);

            var size = ImageHelper.SaveImageFileAsJpeg(sourceFilePath, newFilePath, gallerySetting.MaxThumbnailLength, true, gallerySetting.ThumbnailImageJpegQuality);

            GalleryObject.Thumbnail.Width = size.Width;
            GalleryObject.Thumbnail.Height = size.Height;

            GalleryObject.Thumbnail.FileName = newFilename;
            GalleryObject.Thumbnail.FileNamePhysicalPath = newFilePath;

            int fileSize = (int)(GalleryObject.Thumbnail.FileInfo.Length / 1024);

            GalleryObject.Thumbnail.FileSizeKB = (fileSize < 1 ? 1 : fileSize); // Very small files should be 1, not 0.
        }

        private static string GetGenericThumbnailFilePath(IMimeType mimeType)
        {
            var basePath = Path.Combine(AppSetting.Instance.WebRootPath, GlobalConstants.GenericThumbnailDirectory);

            switch (mimeType.MajorType.ToUpperInvariant())
            {
                case "AUDIO": return Path.Combine(basePath, GlobalConstants.GenericAudioThumbnailFileName);
                case "VIDEO": return Path.Combine(basePath, GlobalConstants.GenericVideoThumbnailFileName);
                case "IMAGE": return Path.Combine(basePath, GlobalConstants.GenericImageThumbnailFileName);
                default: return Path.Combine(basePath, GlobalConstants.GenericUnknownThumbnailFileName);
            }
        }

        //private static Bitmap GetGenericThumbnailBitmap(IMimeType mimeType)
        //{
        //    Bitmap thumbnailBitmap;

        //    switch (mimeType.MajorType.ToUpperInvariant())
        //    {
        //        case "AUDIO": thumbnailBitmap = new Bitmap(new MemoryStream(Resources.GenericThumbnailImage_Audio)); break;
        //        case "VIDEO": thumbnailBitmap = new Bitmap(new MemoryStream(Resources.GenericThumbnailImage_Video)); break;
        //        case "IMAGE": thumbnailBitmap = new Bitmap(new MemoryStream(Resources.GenericThumbnailImage_Image)); break;
        //        default: thumbnailBitmap = new Bitmap(new MemoryStream(Resources.GenericThumbnailImage_Unknown)); break;
        //    }

        //    return thumbnailBitmap;
        //}

        private bool IsThumbnailImageRequired()
        {
            // We must create a thumbnail image in the following circumstances:
            // 1. The file corresponding to a previously created thumbnail image file does not exist.
            //    OR
            // 2. The overwrite flag is true.

            bool thumbnailImageMissing = IsThumbnailImageFileMissing(); // Test 1

            bool overwriteFlag = GalleryObject.RegenerateThumbnailOnSave; // Test 2

            return (thumbnailImageMissing || overwriteFlag);
        }

        private bool IsThumbnailImageFileMissing()
        {
            // Does the thumbnail image file exist? (Maybe it was accidentally deleted or moved by the user,
            // or maybe it's a new object.)
            bool thumbnailImageExists = false;

            if (File.Exists(GalleryObject.Thumbnail.FileNamePhysicalPath))
            {
                // Thumbnail image file exists.
                thumbnailImageExists = true;
            }

            bool thumbnailImageIsMissing = !thumbnailImageExists;

            return thumbnailImageIsMissing;
        }

        private static string GenerateNewFilename(string filenamePrefix)
        {
            return String.Format(CultureInfo.CurrentCulture, "{0}{1}.jpg", filenamePrefix, GlobalConstants.ExternalMediaObjectFilename);
        }
    }
}