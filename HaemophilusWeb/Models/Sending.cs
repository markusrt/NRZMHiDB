using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FluentValidation.Attributes;
using HaemophilusWeb.Validators;

namespace HaemophilusWeb.Models
{
    [Validator(typeof (SendingValidator))]
    public class Sending
    {
        public Sending()
        {
            ReceivingDate = DateTime.Now;
            SamplingDate = DateTime.Now.Subtract(TimeSpan.FromDays(7));
            SenderConclusion = "H.influenzae";
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Einsendungsnummer")]
        public int SendingId { get; set; }

        [Display(Name = "Einsender")]
        public int SenderId { get; set; }

        [Display(Name = "Patient")]
        [ForeignKey("Patient")]
        public int PatientId { get; set; }

        public virtual Patient Patient { get; set; }

        [Display(Name = "Entnahmedatum")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? SamplingDate { get; set; }

        [Display(Name = "Eingangsdatum")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime ReceivingDate { get; set; }

        [Display(Name = "Entnahmeort")]
        public SamplingLocation SamplingLocation { get; set; }

        [Display(Name = "Anderer Entnahmeort")]
        public string OtherSamplingLocation { get; set; }

        [Display(Name = "Material")]
        public Material Material { get; set; }

        [Display(Name = "Invasiv")]
        public YesNo? Invasive { get; set; }

        [Display(Name = "Labnr. Einsender")]
        public string SenderLaboratoryNumber { get; set; }

        [Display(Name = "Ergebnis Einsender")]
        public string SenderConclusion { get; set; }

        [Display(Name = "Bemerkung")]
        public string Remark { get; set; }

        public virtual Isolate Isolate { get; set; }

        [NotMapped]
        [Display(Name = "Labornummer")]
        public string LaboratoryNumber { get; set; }
    }
}