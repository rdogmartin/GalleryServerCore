using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GalleryServer.Data
{
    [Table("Role", Schema = "gsp")]
    public class RoleDto
    {
        [Key, MaxLength(256)]
        public string RoleName
        {
            get;
            set;
        }

        [Required]
        public bool AllowViewAlbumsAndObjects
        {
            get;
            set;
        }

        [Required]
        public bool AllowViewOriginalImage
        {
            get;
            set;
        }

        [Required]
        public bool AllowAddChildAlbum
        {
            get;
            set;
        }

        [Required]
        public bool AllowAddMediaObject
        {
            get;
            set;
        }

        [Required]
        public bool AllowEditAlbum
        {
            get;
            set;
        }

        [Required]
        public bool AllowEditMediaObject
        {
            get;
            set;
        }

        [Required]
        public bool AllowDeleteChildAlbum
        {
            get;
            set;
        }

        [Required]
        public bool AllowDeleteMediaObject
        {
            get;
            set;
        }

        [Required]
        public bool AllowSynchronize
        {
            get;
            set;
        }

        [Required]
        public bool HideWatermark
        {
            get;
            set;
        }

        [Required]
        public bool AllowAdministerGallery
        {
            get;
            set;
        }

        [Required]
        public bool AllowAdministerSite
        {
            get;
            set;
        }

        [InverseProperty("Role")]
        public ICollection<RoleAlbumDto> RoleAlbums
        {
            get;
            set;
        }
    }
}
