using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GalleryServer.Business;

namespace GalleryServer.Data
{
	[Table("MediaQueue", Schema = "gsp")]
	public class MediaQueueDto
	{
		[Key]
		public int MediaQueueId
		{
			get;
			set;
		}

		[Required]
		public int FKMediaObjectId
		{
			get;
			set;
		}

		[ForeignKey("FKMediaObjectId")]
		public MediaObjectDto MediaObject
		{
			get;
			set;
		}

		[Required, MaxLength(256)]
		public string Status
		{
			get;
			set;
		}

		[Required(AllowEmptyStrings = true), MaxLength]
		public string StatusDetail
		{
			get;
			set;
		}

		[Required]
		public MediaQueueItemConversionType ConversionType { get; set; }

		[Required]
		public MediaAssetRotateFlip RotationAmount { get; set; }

		[Required]
        [Column(TypeName = "datetime")]
		public System.DateTime DateAdded
		{
			get;
			set;
		}

        [Column(TypeName = "datetime")]
		public System.DateTime? DateConversionStarted
		{
			get;
			set;
		}

        [Column(TypeName = "datetime")]
		public System.DateTime? DateConversionCompleted
		{
			get;
			set;
		}
	}
}
