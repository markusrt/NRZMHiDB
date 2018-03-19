using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaemophilusWeb.Models
{
    public class RkiMatchRecord
    {
        [Display(Name = "Einsendung")]
        [Key]
        [ForeignKey("Sending")]
        public int SendingId { get; set; }

        [Display(Name = "InterneRef")]
        public int RkiReferenceId { get; set; }

        [Display(Name = "RKI Aktenzeichen")]
        public string RkiReferenceNumber { get; set; }

        [Display(Name = "RKI Status")]
        public RkiStatus RkiStatus { get; set; }

        public virtual Sending Sending { get; set; }
    }
}