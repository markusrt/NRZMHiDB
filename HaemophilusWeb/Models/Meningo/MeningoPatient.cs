using System.ComponentModel.DataAnnotations;

namespace HaemophilusWeb.Models.Meningo
{
    public class MeningoPatient : PatientBase
    {
        [Display(Name = "Klinische Angaben")]
        public MeningoClinicalInformation ClinicalInformation { get; set; }

        [Display(Name = "Andere kl. Angaben")]
        public string OtherClinicalInformation { get; set; }

        [Display(Name = "Grunderkrankung")]
        public UnderlyingDisease UnderlyingDisease { get; set; }

        [Display(Name = "Andere Grunderkrankung")]
        public string OtherUnderlyingDisease { get; set; }

        [Display(Name = "Residuen")]
        public string Residua { get; set; }

        [Display(Name = "Epidemiologie")]
        public Epidemiology Epidemiology { get; set; }
    }
}