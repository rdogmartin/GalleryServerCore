using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GalleryServer.Data
{
    [Table("AppSetting", Schema = "gsp")]
    public class AppSettingDto
    {
        [Key]
        public int AppSettingId
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
