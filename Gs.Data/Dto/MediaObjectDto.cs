using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GalleryServer.Data
{
	[Table("MediaObject", Schema = "gsp")]
	public class MediaObjectDto
	{
		[Key]
		public int MediaObjectId
		{
			get;
			set;
		}

		[Required]
		public int FKAlbumId
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

		[Required(AllowEmptyStrings = true), MaxLength(255)]
		public string ThumbnailFilename
		{
			get;
			set;
		}

		[Required]
		public int ThumbnailWidth
		{
			get;
			set;
		}

		[Required]
		public int ThumbnailHeight
		{
			get;
			set;
		}

		[Required]
		public int ThumbnailSizeKB
		{
			get;
			set;
		}

		[Required(AllowEmptyStrings = true), MaxLength(255)]
		public string OptimizedFilename
		{
			get;
			set;
		}

		[Required]
		public int OptimizedWidth
		{
			get;
			set;
		}

		[Required]
		public int OptimizedHeight
		{
			get;
			set;
		}

		[Required]
		public int OptimizedSizeKB
		{
			get;
			set;
		}

		[Required(AllowEmptyStrings = true), MaxLength(255)]
		public string OriginalFilename
		{
			get;
			set;
		}

		[Required]
		public int OriginalWidth
		{
			get;
			set;
		}

		[Required]
		public int OriginalHeight
		{
			get;
			set;
		}

		[Required]
		public int OriginalSizeKB
		{
			get;
			set;
		}

		[Required(AllowEmptyStrings = true), MaxLength]
		public string ExternalHtmlSource
		{
			get;
			set;
		}

		[Required(AllowEmptyStrings = true), MaxLength(15)]
		public string ExternalType
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

		[Required, MaxLength(256)]
		public string CreatedBy
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

		[Required]
		public bool IsPrivate
		{
			get;
			set;
		}

        [InverseProperty("MediaObject")]
		public ICollection<MetadataDto> Metadata
		{
			get;
			set;
		}
	}
}
