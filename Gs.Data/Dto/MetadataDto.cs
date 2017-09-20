using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GalleryServer.Business.Metadata;

namespace GalleryServer.Data
{
	[Table("Metadata", Schema = "gsp")]
	[System.Diagnostics.DebuggerDisplay("Meta {MetaName} = {Value}")]
	public class MetadataDto
	{
		[Key]
		public int MetadataId
		{
			get;
			set;
		}

		[Required]
		public MetadataItemName MetaName
		{
			get;
			set;
		}

		public int? FKMediaObjectId
		{
			get;
			set;
		}

		public int? FKAlbumId
		{
			get;
			set;
		}

		[ForeignKey("FKMediaObjectId")]
        [InverseProperty("Metadata")]
		public MediaObjectDto MediaObject
		{
			get;
			set;
		}

		[ForeignKey("FKAlbumId")]
        [InverseProperty("Metadata")]
		public AlbumDto Album
		{
			get;
			set;
		}

		[MaxLength]
		public string RawValue
		{
			get;
			set;
		}

		[Required(AllowEmptyStrings = true), MaxLength]
		public string Value
		{
			get;
			set;
		}

        [InverseProperty("Metadata")]
		public ICollection<MetadataTagDto> MetadataTags
		{
			get;
			set;
		}
	}
}
