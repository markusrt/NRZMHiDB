using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FluentValidation.Attributes;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;
using HaemophilusWeb.Validators;

namespace HaemophilusWeb.Models
{
    [Validator(typeof (SendingValidator))]
    public class Sending : SendingBase<Patient>
    {
        public Sending() : base(DatabaseType.Haemophilus)
        {
            ReceivingDate = DateTime.Now;
            SamplingDate = DateTime.Now.Subtract(TimeSpan.FromDays(7));
            SenderSpecies = "H. influenzae";
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

        [Display(Name = "Invasiv")]
        public YesNo? Invasive => IsInvasive(SamplingLocation)
            ? YesNo.Yes
            : YesNo.No;

        public static bool IsInvasive(SamplingLocation samplingLocation)
        {
            return samplingLocation.FirstAttribute<InvasiveSamplingLocationAttribute>() != null;
        }

        [Display(Name = "Anderer Entnahmeort")]
        public string OtherSamplingLocation { get; set; }

        [Display(Name = "Serotyp Einsender")]
        public string SerotypeSender { get; set; }

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

        public override IsolateCommon GetIsolate()
        {
            return Isolate;
        }

        public override IsolateCommon CreateIsolate()
        {
            return Isolate = new Isolate();
        }
    }
}