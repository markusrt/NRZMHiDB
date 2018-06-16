using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HaemophilusWeb.Models.Meningo
{
    //TODO Validator
    public class MeningoSending : SendingBase<MeningoPatient>
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

        [Display(Name = "Serogruppe Einsender")]
        public string SerogroupSender { get; set; }


        //public virtual Isolate Isolate { get; set; }

        public override void SetPatientId(int patientId)
        {
            MeningoPatientId = patientId;
        }

        public override int GetSendingId()
        {
            return MeningoSendingId;
        }

        public override Isolate GetIsolate()
        {
            //TODO move to IsolateBase
            return null;
        }

        public override void SetIsolate(Isolate isolate)
        {
            //TODO move to IsolateBase
        }
    }
}