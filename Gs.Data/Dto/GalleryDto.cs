using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GalleryServer.Data
{
    [Table("Gallery", Schema = "gsp")]
    public class GalleryDto
    {
        [Key]
        public int GalleryId
        {
            get;
            set;
        }

        [Required(AllowEmptyStrings = true), MaxLength(1000)]
        public string Description
        {
            get;
            set;
        }

        [Required]
        public bool IsTemplate
        {
            get;
            set;
        }

        [Required]
        [Column(TypeName = "datetime")]
        public System.DateTime DateAdded
        {
            get;
            set;
        }
    }
}
