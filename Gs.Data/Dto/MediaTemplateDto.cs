using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GalleryServer.Data
{
    [Table("MediaTemplate", Schema = "gsp")]
    public class MediaTemplateDto
    {
        [Key]
        public int MediaTemplateId
        {
            get;
            set;
        }

        [Required, MaxLength(200)]
        public string MimeType
        {
            get;
            set;
        }

        [Required, MaxLength(50)]
        public string BrowserId
        {
            get;
            set;
        }

        [Required(AllowEmptyStrings = true), MaxLength]
        public string HtmlTemplate
        {
            get;
            set;
        }

        [Required(AllowEmptyStrings = true), MaxLength]
        public string ScriptTemplate
        {
            get;
            set;
        }
    }
}
