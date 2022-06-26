using System.ComponentModel.DataAnnotations;
using HaemophilusWeb.Models;

namespace HaemophilusWeb.ViewModels
{
    public class EpsilometerTestViewModel
    {
        public EpsilometerTestViewModel()
        {
            
        }

        public EpsilometerTestViewModel(Antibiotic antibiotic)
        {
            Antibiotic = antibiotic;
        }

        [Display(Name = "Antibiotikum")]
        public Antibiotic? Antibiotic { get; set; }

        [Display(Name = "Breakpoint")]
        public int? EucastClinicalBreakpointId { get; set; }

        [Display(Name = "Messwert")]
        public float? Measurement { get; set; }

        public EpsilometerTestResult? Result { get; set; }

        public float? MicBreakpointSusceptible { get; set; }

        public float? MicBreakpointResistent { get; set; }

        public int? ValidFromYear { get; set; }
        public bool ReadonlyAntibiotic { get; set; }
    }
}