using System.ComponentModel.DataAnnotations;

namespace HaemophilusWeb.Models.Meningo
{
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