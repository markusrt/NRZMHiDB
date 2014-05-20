using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using HaemophilusWeb.Models;
using HaemophilusWeb.Validators;
using HaemophilusWeb.Views.Utils;

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
        
        public IEnumerable<Typing> Typings
        {
            get
            {
                if (OuterMembraneProteinP2 != 0)
                {
                    yield return new Typing { Attribute = "ompP2", Value = EnumEditor.GetEnumDescription(OuterMembraneProteinP2) };
                }
                if (OuterMembraneProteinP6 != 0)
                {
                    yield return new Typing { Attribute = "ompP6", Value = EnumEditor.GetEnumDescription(OuterMembraneProteinP6) };
                }
            }
        } 

        public IEnumerable<EpsilometerTestReportModel> ETests
        {
            get
            {
                if (Ampicillin.EucastClinicalBreakpointId != null && Ampicillin.Result.HasValue)
                {
                    yield return CreateEpsilometerTestReportModel(Ampicillin);
                }
                if (AmoxicillinClavulanate.EucastClinicalBreakpointId != null && AmoxicillinClavulanate.Result.HasValue)
                {
                    yield return CreateEpsilometerTestReportModel(AmoxicillinClavulanate);
                }
                if (Cefotaxime.EucastClinicalBreakpointId != null && Cefotaxime.Result.HasValue)
                {
                    yield return CreateEpsilometerTestReportModel(Cefotaxime);
                }
                if (Meropenem.EucastClinicalBreakpointId != null && Meropenem.Result.HasValue)
                {
                    yield return CreateEpsilometerTestReportModel(Meropenem);
                }
            }
        }

        public string AgglutinationString
        {
            get { return EnumEditor.GetEnumDescription(Agglutination); }
        }

        private EpsilometerTestReportModel CreateEpsilometerTestReportModel(EpsilometerTestViewModel epsilometerTestViewModel)
        {
            var reportModel = new EpsilometerTestReportModel
            {
                Antibiotic =  EnumEditor.GetEnumDescription(epsilometerTestViewModel.Antibiotic),
                Measurement = epsilometerTestViewModel.Measurement,
                Result = EnumEditor.GetEnumDescription(epsilometerTestViewModel.Result)
            };
            return reportModel;
        }
    }

    public class Typing
    {
        public string Attribute { get; set; }
        public string Value { get; set; }
    }

    public class EpsilometerTestReportModel
    {
        public string Antibiotic { get; set; }
        public string Result { get; set; }
        public double MicBreakpointSusceptible { get; set; }
        public double MicBreakpointResistent { get; set; }
        public double Measurement { get; set; }
    }
}