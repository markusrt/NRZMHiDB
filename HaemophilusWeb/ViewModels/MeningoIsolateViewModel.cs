using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HaemophilusWeb.Models.Meningo;

namespace HaemophilusWeb.ViewModels
{
    public class MeningoIsolateViewModel : MeningoIsolateBase
    {
        private List<EpsilometerTestViewModel> etests;

        public MeningoIsolateViewModel()
        {
            EpsilometerTestViewModels = new List<EpsilometerTestViewModel>();
        }

        public ICollection<EpsilometerTestViewModel> EpsilometerTestViewModels { get; set; }

        [Display(Name = "Invasiv")]
        public string Invasive { get; set; }

        [Display(Name = "Entnahmeort")]
        public string SamplingLocation { get; set; }

        [Display(Name = "Material")]
        public string Material { get; set; }

        [Display(Name = "Patientenalter bei Entnahme")]
        public int PatientAgeAtSampling { get; set; }
    }
}