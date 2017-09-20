using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GalleryServer.Data
{
    [Table("MimeTypeGallery", Schema = "gsp")]
    public class MimeTypeGalleryDto
    {
        [Key]
        public int MimeTypeGalleryId
        {
            get;
            set;
        }

        [Required]
        public int FKGalleryId
        {
            get;
            set;
        }

        [Required]
        public int FKMimeTypeId
        {
            get;
            set;
        }

        [Required]
        public bool IsEnabled
        {
            get;
            set;
        }

        [ForeignKey("FKGalleryId")]
        public GalleryDto Gallery
        {
            get;
            set;
        }

        [ForeignKey("FKMimeTypeId")]
        [InverseProperty("MimeTypeGalleries")]
        public MimeTypeDto MimeType
        {
            get;
            set;
        }
    }
}
