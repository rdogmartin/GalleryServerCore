using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GalleryServer.Data
{
    [Table("UserGalleryProfile", Schema = "gsp")]
    public class UserGalleryProfileDto
    {
        [Key]
        public int ProfileId
        {
            get;
            set;
        }

        [Required, MaxLength(256)]
        public string UserName
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
