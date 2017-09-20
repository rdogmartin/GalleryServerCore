using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GalleryServer.Data
{
    [Table("RoleAlbum", Schema = "gsp")]
    public class RoleAlbumDto
    {
        [Key, Column(Order = 0), MaxLength(256)]
        public string FKRoleName
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

        [ForeignKey("FKRoleName")]
        [InverseProperty("RoleAlbums")]
        public RoleDto Role
        {
            get;
            set;
        }

        [ForeignKey("FKAlbumId")]
        public AlbumDto Album
        {
            get;
            set;
        }
    }
}
