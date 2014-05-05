using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Script.Serialization;
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

        public int IsolateId { get; set; }

        [Display(Name = "Stammnummer")]
        public int? StemNumber { get; set; }

        [Display(Name = "Labornummer")]
        public string LaboratoryNumber { get; set; }

        [Display(Name = "Faktoren-test")]
        public FactorTest? FactorTest { get; set; }

        public Serotype? Agglutination { get; set; }

        [Display(Name = "ß-Laktamase")]
        public TestResult? BetaLactamase { get; set; }

        public TestResult? Oxidase { get; set; }

        [Display(Name = "opm-P2")]
        public TestResult? OuterMembraneProteinP2 { get; set; }

        [Display(Name = "fucK")]
        public TestResult FuculoKinase { get; set; }

        [Display(Name = "opm-P6")]
        public TestResult OuterMembraneProteinP6 { get; set; }

        [Display(Name = "bexA")]
        public TestResult? BexA { get; set; }

        [Display(Name = "Serotyp-PCR")]
        public Serotype? SerotypePcr { get; set; }

        [Display(Name = "16S rRNA")]
        public TestResult RibosomalRna16S { get; set; }

        [Display(Name = "api NH")]
        public TestResult? ApiNh { get; set; }

        public EpsilometerTestViewModel Ampicillin { get; private set; }

        [Display(Name = "Amoxicillin / Clavulansäure")]
        public EpsilometerTestViewModel AmoxicillinClavulanate { get; private set; }

        [Display(Name = "Cefotaxim")]
        public EpsilometerTestViewModel Cefotaxime { get; private set; }

        public EpsilometerTestViewModel Meropenem { get; private set; }

        [Display(Name = "Material")]
        public string Material { get; set; }

        [Display(Name = "Invasiv")]
        public string Invasive { get; set; }

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