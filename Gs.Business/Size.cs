using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business
{
    public struct Size: ISize
    {
        /// <summary>
        /// Represents a <see cref="ISize" /> that has Width and Height values set to zero.
        /// </summary>
        public static readonly ISize Empty = new Size();

        /// <summary>
        /// Initializes a new instance of the <see cref="Size" /> struct.
        /// </summary>
        /// <param name="width">The width of the size.</param>
        /// <param name="height">The height of the size.</param>
        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <inheritdoc />
        public int Width { get; set; }

        /// <inheritdoc />
        public int Height { get; set; }

        /// <inheritdoc />
        public bool IsEmpty => Equals(Empty);
    }
}
