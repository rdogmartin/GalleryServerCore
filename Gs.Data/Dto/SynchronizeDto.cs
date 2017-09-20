using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Data
{
    [Table("Synchronize", Schema = "gsp")]
    public class SynchronizeDto
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int FKGalleryId
        {
            get;
            set;
        }

        [Required(AllowEmptyStrings = true), MaxLength(46)]
        public string SynchId
        {
            get;
            set;
        }

        [Required]
        public SynchronizationState SynchState
        {
            get;
            set;
        }

        [Required]
        public int TotalFiles
        {
            get;
            set;
        }

        [Required]
        public int CurrentFileIndex
        {
            get;
            set;
        }
    }
}
