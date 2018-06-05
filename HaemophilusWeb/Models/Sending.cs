using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FluentValidation.Attributes;
using HaemophilusWeb.Validators;

namespace HaemophilusWeb.Models
{
    [Validator(typeof (SendingValidator))]
    public class Sending : SendingBase<Patient>
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

        [Display(Name = "Material")]
        public Material Material { get; set; }

        [Display(Name = "Bemerkung")]
        public string Remark { get; set; }

        public virtual RkiMatchRecord RkiMatchRecord { get; set; }
        public virtual Isolate Isolate { get; set; }

        public override void SetPatientId(int patientId)
        {
            PatientId = patientId;
        }

        public override int GetSendingId()
        {
            return SendingId;
        }

        public override Isolate GetIsolate()
        {
            return Isolate;
        }

        public override void SetIsolate(Isolate isolate)
        {
            Isolate = isolate;
        }
    }
}