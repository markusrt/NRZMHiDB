using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentValidation.Attributes;
using HaemophilusWeb.Models;
using HaemophilusWeb.Validators;

namespace HaemophilusWeb.ViewModels
{
    [Validator(typeof (IsolateViewModelValidator))]
    public class IsolateViewModel : IsolateBase
    {
        public IsolateViewModel()
        {
            Ampicillin = new EpsilometerTestViewModel(Antibiotic.Ampicillin);
            AmoxicillinClavulanate = new EpsilometerTestViewModel(Antibiotic.AmoxicillinClavulanate);
            Cefotaxime = new EpsilometerTestViewModel(Antibiotic.Cefotaxime);
            Meropenem = new EpsilometerTestViewModel(Antibiotic.Meropenem);
        }

        [Display(Name = "Labornummer")]
        public string LaboratoryNumber { get; set; }

        [Display(Name = "Invasiv")]
        public string Invasive { get; set; }

        [Display(Name = "Material")]
        public string Material { get; set; }

        public EpsilometerTestViewModel Ampicillin { get; private set; }

        [Display(Name = "Amoxicillin / Clavulansäure")]
        public EpsilometerTestViewModel AmoxicillinClavulanate { get; private set; }

        [Display(Name = "Cefotaxim")]
        public EpsilometerTestViewModel Cefotaxime { get; private set; }

        public EpsilometerTestViewModel Meropenem { get; private set; }

        [Display(Name = "Patientenalter bei Entnahme")]
        public int PatientAgeAtSampling { get; set; }

        public IEnumerable<EpsilometerTestViewModel> EpsilometerTestViewModels
        {
            get
            {
                yield return Ampicillin;
                yield return AmoxicillinClavulanate;
                yield return Cefotaxime;
                yield return Meropenem;
            }
        }
    }
}