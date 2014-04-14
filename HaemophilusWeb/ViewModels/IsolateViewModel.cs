using System.ComponentModel.DataAnnotations;
using HaemophilusWeb.Models;
using FluentValidation.Attributes;
using HaemophilusWeb.Validators;

namespace HaemophilusWeb.ViewModels
{
    [Validator(typeof(IsolateViewModelValidator))]
    public class IsolateViewModel
    {
        public IsolateViewModel()
        {
            Ampicillin = new EpsilometerTestViewModel(Antibiotic.Ampicillin);
            AmoxicillinClavulanate = new EpsilometerTestViewModel(Antibiotic.AmoxicillinClavulanate);
            Cefotaxime = new EpsilometerTestViewModel(Antibiotic.Cefotaxime);
            Meropenem = new EpsilometerTestViewModel(Antibiotic.Meropenem);
        }

        public Isolate TheIsolate { get; set; }

        public EpsilometerTestViewModel Ampicillin { get; private set; }

        [Display(Name = "Amoxicillin / Clavulansäure")]
        public EpsilometerTestViewModel AmoxicillinClavulanate { get; private set; }

        [Display(Name = "Cefotaxim")]
        public EpsilometerTestViewModel Cefotaxime { get; private set; }

        public EpsilometerTestViewModel Meropenem { get; private set; }
    }
}