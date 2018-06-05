using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using HaemophilusWeb.Models;

namespace HaemophilusWeb.ViewModels
{
    public class PatientSendingViewModel<TPatient, TSending>
        where TPatient : PatientBase 
        where TSending : SendingBase<TPatient>
    {
        public TPatient Patient { get; set; }

        public TSending Sending { get; set; }

        public bool DuplicatePatientDetected { get; set; }

        [Display(Name = "Konfliktlösung")]
        public DuplicatePatientResolution? DuplicatePatientResolution { get; set; }
    }

    public enum DuplicatePatientResolution
    {
        [Description("Neuen Patienten anlegen")]
        CreateNewPatient=0,
        [Description("Bestehenden Patienten verwenden")]
        UseExistingPatient=1
    }
}