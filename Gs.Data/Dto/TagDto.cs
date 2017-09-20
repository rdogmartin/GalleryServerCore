using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GalleryServer.Data
{
	[Table("Tag", Schema = "gsp")]
	[System.Diagnostics.DebuggerDisplay("{" + nameof(TagName) + "}")]
	public class TagDto
	{
		[Key, MaxLength(100), DatabaseGenerated(DatabaseGeneratedOption.None)]
		public string TagName
		{
			get;
			set;
		}

        [InverseProperty("Tag")]
		public ICollection<MetadataTagDto> MetadataTags
		{
			get;
			set;
		}
	}
}
