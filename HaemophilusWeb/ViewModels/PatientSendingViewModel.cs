using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using HaemophilusWeb.Models;

namespace HaemophilusWeb.ViewModels
{
    public class PatientSendingViewModel
    {
        public Patient Patient { get; set; }

        public Sending Sending { get; set; }

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