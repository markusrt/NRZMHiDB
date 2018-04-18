using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaemophilusWeb.Models
{
    public class SendingBase
    {
        [Display(Name = "Einsendernummer")]
        public int SenderId { get; set; }

        [Display(Name = "Entnahmedatum")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? SamplingDate { get; set; }

        [Display(Name = "Eingangsdatum")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ReceivingDate { get; set; }

        [NotMapped]
        [Display(Name = "Labornummer")]
        public string LaboratoryNumber { get; set; }

        [Display(Name = "Gelöscht")]
        public bool Deleted { get; set; }

        [Display(Name = "Labnr. Einsender")]
        public string SenderLaboratoryNumber { get; set; }

        [Display(Name = "Invasiv")]
        public YesNo? Invasive { get; set; }

        [Display(Name = "Ergebnis Einsender")]
        public string SenderConclusion { get; set; }
    }
}