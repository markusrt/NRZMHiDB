using System.ComponentModel.DataAnnotations;
using HaemophilusWeb.Models;

namespace HaemophilusWeb.ViewModels
{
    public class EpsilometerTestViewModel
    {
        public EpsilometerTestViewModel(Antibiotic antibiotic)
        {
            Antibiotic = antibiotic;
        }

        public Antibiotic Antibiotic { get; private set; }

        public int? EucastClinicalBreakpointId { get; set; }

        [Display(Name = "Messwert")]
        public float Measurement { get; set; }

        public EpsilometerTestResult? Result { get; set; } 
    }
}