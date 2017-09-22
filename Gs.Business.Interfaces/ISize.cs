namespace GalleryServer.Business.Interfaces
{
    public interface ISize
    {
        ISize Empty { get; set; }

        double Height { get; set; }

        bool IsEmpty { get; }

        double Width { get; set; }
    }
}
