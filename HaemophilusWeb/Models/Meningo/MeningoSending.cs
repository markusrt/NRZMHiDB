using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaemophilusWeb.Models.Meningo
{
    //TODO Validator
    public class MeningoSending : SendingBase
    {
        public MeningoSending()
        {
            ReceivingDate = DateTime.Now;
            SamplingDate = DateTime.Now.Subtract(TimeSpan.FromDays(7));
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Display(Name = "Einsendungsnummer")]
        public int MeningoSendingId { get; set; }

        [Display(Name = "Patient")]
        [ForeignKey("Patient")]
        public int MeningoPatientId { get; set; }

        [Display(Name = "Entnahmeort")]
        public MeningoSamplingLocation SamplingLocation { get; set; }

        [Display(Name = "Material")]
        public MeningoMaterial Material { get; set; }

        [Display(Name = "Anderer Entnahmeort")]
        public string OtherSamplingLocation { get; set; }

        //public virtual Isolate Isolate { get; set; }
        public virtual MeningoPatient Patient { get; set; }
    }
}