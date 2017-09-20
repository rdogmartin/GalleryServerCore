using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GalleryServer.Business.Metadata;

namespace GalleryServer.Data
{
	[Table("Album", Schema = "gsp")]
	public class AlbumDto
	{
		[Key]
		public int AlbumId
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

		public int? FKAlbumParentId
		{
			get;
			set;
		}

		[ForeignKey("FKAlbumParentId")]
		public AlbumDto AlbumParent
		{
			get;
			set;
		}

		[Required(AllowEmptyStrings = true), MaxLength(255)]
		public string DirectoryName
		{
			get;
			set;
		}

		[Required]
		public int ThumbnailMediaObjectId
		{
			get;
			set;
		}

		[Required]
		public MetadataItemName SortByMetaName
		{
			get;
			set;
		}

		[Required]
		public bool SortAscending
		{
			get;
			set;
		}

		[Required]
		public int Seq
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

		[Required, MaxLength(256)]
		public string CreatedBy
		{
			get;
			set;
		}

		[Required, MaxLength(256)]
		public string LastModifiedBy
		{
			get;
			set;
		}

		[Required]
        [Column(TypeName = "datetime")]
		public System.DateTime DateLastModified
		{
			get;
			set;
		}

		[Required(AllowEmptyStrings = true), MaxLength(256)]
		public string OwnedBy
		{
			get;
			set;
		}

		[Required(AllowEmptyStrings = true), MaxLength(256)]
		public string OwnerRoleName
		{
			get;
			set;
		}

		[Required]
		public bool IsPrivate
		{
			get;
			set;
		}

        [InverseProperty("Album")]
		public ICollection<MetadataDto> Metadata
		{
			get;
			set;
		}

        [InverseProperty("Album")]
		public ICollection<UiTemplateAlbumDto> UiTemplates
		{
			get;
			set;
		}
	}
}
