using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GalleryServer.Data
{
    [Table("GalleryControlSetting", Schema = "gsp")]
    public class GalleryControlSettingDto
    {
        [Key]
        public int GalleryControlSettingId
        {
            get;
            set;
        }

        [Required, MaxLength(350)]
        public string ControlId
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
