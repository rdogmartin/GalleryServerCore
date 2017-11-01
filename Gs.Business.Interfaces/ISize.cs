namespace GalleryServer.Business.Interfaces
{
    public interface ISize
    {
        /// <summary>
        /// Gets or sets the width of this instance.
        /// </summary>
        int Width { get; set; }

        /// <summary>
        /// Gets or sets the height of this instance.
        /// </summary>
        int Height { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        bool IsEmpty { get; }
    }
}
