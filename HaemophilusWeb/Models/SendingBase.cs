using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;

namespace HaemophilusWeb.Models
{
    public abstract class SendingBase<TPatient>
    {
        [NotMapped]
        public DatabaseType DatabaseType { get; }

        public SendingBase(DatabaseType databaseType)
        {
            DatabaseType = databaseType;
        }

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

        [NotMapped]
        [Display(Name = "Labornummer")]
        public string LaboratoryNumberWithPrefix => LaboratoryNumber.ToLaboratoryNumberWithPrefix(DatabaseType);

        [Display(Name = "Gelöscht")]
        public bool Deleted { get; set; }

        [Display(Name = "Labnr. Einsender")]
        public string SenderLaboratoryNumber { get; set; }

        [Display(Name = "DEMIS ID")]
        public Guid? DemisId { get; set; }

        [Display(Name = "Bemerkung")]
        public string Remark { get; set; }

        [NotMapped]
        public string IsolateLaboratoryNumber => GetIsolate()?.LaboratoryNumber;

        [NotMapped]
        public virtual bool AutoAssignStemNumber => false;

        public virtual TPatient Patient { get; set; }

        public abstract void SetPatientId(int patientId);

        public abstract int GetSendingId();

        public abstract IsolateCommon GetIsolate();

        public abstract IsolateCommon CreateIsolate();
    }
}