using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentValidation.Attributes;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Validators;

namespace HaemophilusWeb.ViewModels
{
    [Validator(typeof(MeningoIsolateViewModelValidator))]
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