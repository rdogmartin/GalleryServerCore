using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GalleryServer.Data
{
    [Table("MimeType", Schema = "gsp")]
    public class MimeTypeDto
    {
        [Key]
        public int MimeTypeId
        {
            get;
            set;
        }

        [Required, MaxLength(30)]
        public string FileExtension
        {
            get;
            set;
        }

        [Required, MaxLength(200)]
        public string MimeTypeValue
        {
            get;
            set;
        }

        [Required(AllowEmptyStrings = true), MaxLength(200)]
        public string BrowserMimeTypeValue
        {
            get;
            set;
        }

        [InverseProperty("MimeType")]
        public ICollection<MimeTypeGalleryDto> MimeTypeGalleries
        {
            get;
            set;
        }
    }
}
