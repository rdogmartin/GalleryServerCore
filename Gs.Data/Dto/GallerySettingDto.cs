using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GalleryServer.Data
{
    [Table("GallerySetting", Schema = "gsp")]
    public class GallerySettingDto
    {
        [Key]
        public int GallerySettingId
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

        [ForeignKey("FKGalleryId")]
        public GalleryDto Gallery
        {
            get;
            set;
        }

        [Required, MaxLength(200)]
        public string SettingName
        {
            get;
            set;
        }

        [Required(AllowEmptyStrings = true), MaxLength]
        public string SettingValue
        {
            get;
            set;
        }
    }
}
