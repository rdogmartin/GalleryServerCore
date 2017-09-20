using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GalleryServer.Data
{
    [Table("UiTemplateAlbum", Schema = "gsp")]
    [System.Diagnostics.DebuggerDisplay("Template ID {FKUiTemplateId}; Album ID ({FKAlbumId})")]
    public class UiTemplateAlbumDto
    {
        [Key, Column(Order = 0)]
        public int FKUiTemplateId
        {
            get;
            set;
        }

        [Key, Column(Order = 1)]
        public int FKAlbumId
        {
            get;
            set;
        }

        [ForeignKey("FKUiTemplateId")]
        [InverseProperty("TemplateAlbums")]
        public UiTemplateDto UiTemplate
        {
            get;
            set;
        }

        [ForeignKey("FKAlbumId")]
        [InverseProperty("UiTemplates")]
        public AlbumDto Album
        {
            get;
            set;
        }
    }
}
