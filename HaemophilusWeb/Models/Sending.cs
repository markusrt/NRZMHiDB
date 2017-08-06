using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FluentValidation.Attributes;
using HaemophilusWeb.Validators;

namespace HaemophilusWeb.Models
{
    [Validator(typeof (SendingValidator))]
    public class Sending : SendingBase
    {
        public Sending()
        {
            ReceivingDate = DateTime.Now;
            SamplingDate = DateTime.Now.Subtract(TimeSpan.FromDays(7));
            SenderConclusion = "H. influenzae";
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Einsendungsnummer")]
        public int SendingId { get; set; }

        [Display(Name = "Patient")]
        [ForeignKey("Patient")]
        public int PatientId { get; set; }

        [Display(Name = "Entnahmeort")]
        public SamplingLocation SamplingLocation { get; set; }

        [Display(Name = "Anderer Entnahmeort")]
        public string OtherSamplingLocation { get; set; }

        [Display(Name = "Material")]
        public Material Material { get; set; }

        [Display(Name = "Ergebnis Einsender")]
        public string SenderConclusion { get; set; }

        [Display(Name = "Bemerkung")]
        public string Remark { get; set; }

        public virtual RkiMatchRecord RkiMatchRecord { get; set; }
        public virtual Isolate Isolate { get; set; }
        public virtual Patient Patient { get; set; }
    }
}