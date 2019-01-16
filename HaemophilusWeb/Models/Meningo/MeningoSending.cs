using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FluentValidation.Attributes;
using HaemophilusWeb.Validators;

namespace HaemophilusWeb.Models.Meningo
{
    [Validator(typeof(MeningoSendingValidator))]
    public class MeningoSending : SendingBase<MeningoPatient>
    {
        public MeningoSending()
        {
            ReceivingDate = DateTime.Now;
            SamplingDate = DateTime.Now.Subtract(TimeSpan.FromDays(7));
            SamplingLocation = MeningoSamplingLocation.Blood;
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

        [Display(Name = "Anderer Entnahmeort (invasiv)")]
        public string OtherInvasiveSamplingLocation { get; set; }

        [Display(Name = "Anderer Entnahmeort (nicht invasiv)")]
        public string OtherNonInvasiveSamplingLocation { get; set; }

        [Display(Name = "Invasiv")]
        public YesNo? Invasive => IsInvasive(SamplingLocation)
            ? YesNo.Yes
            : YesNo.No;

        public static bool IsInvasive(MeningoSamplingLocation samplingLocation)
        {
            return samplingLocation.GetType().GetField(Enum.GetName(samplingLocation.GetType(), samplingLocation)).GetCustomAttributes(typeof(InvasiveSamplingLocationAttribute), false).Length > 0;
        }

        public MeningoIsolate Isolate { get; set; }

        public override void SetPatientId(int patientId)
        {
            MeningoPatientId = patientId;
        }

        public override int GetSendingId()
        {
            return MeningoSendingId;
        }

        public override IsolateCommon GetIsolate()
        {
            return Isolate;
        }

        public override IsolateCommon CreateIsolate()
        {
            return Isolate = new MeningoIsolate();
        }
    }
}