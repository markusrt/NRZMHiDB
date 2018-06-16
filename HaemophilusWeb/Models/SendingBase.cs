using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HaemophilusWeb.Models.Meningo;

namespace HaemophilusWeb.Models
{
    public abstract class SendingBase<TPatient>
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

        [Display(Name = "Anderer Entnahmeort")]
        public string OtherSamplingLocation { get; set; }

        [Display(Name = "Bemerkung")]
        public string Remark { get; set; }

        [NotMapped]
        public string IsolateLaboratoryNumber => GetIsolate()?.LaboratoryNumber;

        public virtual TPatient Patient { get; set; }

        public abstract void SetPatientId(int patientId);

        public abstract int GetSendingId();
        public abstract Isolate GetIsolate();
        public abstract void SetIsolate(Isolate isolate);
    }
}