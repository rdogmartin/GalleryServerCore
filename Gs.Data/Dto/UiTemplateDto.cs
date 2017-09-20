using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GalleryServer.Business;

namespace GalleryServer.Data
{
    [Table("UiTemplate", Schema = "gsp")]
    [System.Diagnostics.DebuggerDisplay("{TemplateType} ({Name})")]
    public class UiTemplateDto
    {
        [Key]
        public int UiTemplateId
        {
            get;
            set;
        }

        [Required]
        public UiTemplateType TemplateType
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

        // We can't configure a foreign key because it will conflict with the album relationship in table UiTemplateAlbum
        //[ForeignKey("FKGalleryId")]
        //public GalleryDto Gallery
        //{
        //	get;
        //	set;
        //}

        [Required, MaxLength(255)]
        public string Name
        {
            get;
            set;
        }

        [Required(AllowEmptyStrings = true), MaxLength]
        public string Description
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

        [InverseProperty("UiTemplate")]
        public ICollection<UiTemplateAlbumDto> TemplateAlbums
        {
            get;
            set;
        }
    }
}
