using System.ComponentModel.DataAnnotations;
using FluentValidation.Attributes;
using HaemophilusWeb.Validators;

namespace HaemophilusWeb.Models.Meningo
{
    [Validator(typeof(MeningoPatientValidator))]
    public class MeningoPatient : PatientBase
    {
        [Display(Name = "Klinische Angaben")]
        public MeningoClinicalInformation ClinicalInformation { get; set; }

        [Display(Name = "Andere kl. Angaben")]
        public string OtherClinicalInformation { get; set; }

        [Display(Name = "Risikofaktoren")]
        public RiskFactors RiskFactors { get; set; }

        [Display(Name = "Anderer Risikofaktor")]
        public string OtherRiskFactor { get; set; }
    }
}