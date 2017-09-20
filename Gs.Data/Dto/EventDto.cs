using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GalleryServer.Business;

namespace GalleryServer.Data
{
    [Table("Event", Schema = "gsp")]
    public class EventDto
    {
        [Key]
        public int EventId
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

        [Required]
        public EventType EventType
        {
            get;
            set;
        }

        [Required]
        [Column(TypeName = "datetime")]
        public System.DateTime TimeStampUtc
        {
            get;
            set;
        }

        [Required(AllowEmptyStrings = true), MaxLength(4000)]
        public string Message
        {
            get;
            set;
        }

        [Required(AllowEmptyStrings = true), MaxLength]
        public string EventData
        {
            get;
            set;
        }

        [Required(AllowEmptyStrings = true), MaxLength(1000)]
        public string ExType
        {
            get;
            set;
        }

        [Required(AllowEmptyStrings = true), MaxLength(1000)]
        public string ExSource
        {
            get;
            set;
        }

        [Required(AllowEmptyStrings = true), MaxLength]
        public string ExTargetSite
        {
            get;
            set;
        }

        [Required(AllowEmptyStrings = true), MaxLength]
        public string ExStackTrace
        {
            get;
            set;
        }

        [Required(AllowEmptyStrings = true), MaxLength(1000)]
        public string InnerExType
        {
            get;
            set;
        }

        [Required(AllowEmptyStrings = true), MaxLength(4000)]
        public string InnerExMessage
        {
            get;
            set;
        }

        [Required(AllowEmptyStrings = true), MaxLength(1000)]
        public string InnerExSource
        {
            get;
            set;
        }

        [Required(AllowEmptyStrings = true), MaxLength]
        public string InnerExTargetSite
        {
            get;
            set;
        }

        [Required(AllowEmptyStrings = true), MaxLength]
        public string InnerExStackTrace
        {
            get;
            set;
        }

        [Required(AllowEmptyStrings = true), MaxLength]
        public string InnerExData
        {
            get;
            set;
        }

        [Required(AllowEmptyStrings = true), MaxLength(1000)]
        public string Url
        {
            get;
            set;
        }

        [Required(AllowEmptyStrings = true), MaxLength]
        public string FormVariables
        {
            get;
            set;
        }

        [Required(AllowEmptyStrings = true), MaxLength]
        public string Cookies
        {
            get;
            set;
        }

        [Required(AllowEmptyStrings = true), MaxLength]
        public string SessionVariables
        {
            get;
            set;
        }

        [Required(AllowEmptyStrings = true), MaxLength]
        public string ServerVariables
        {
            get;
            set;
        }
    }
}
