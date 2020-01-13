using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using FluentValidation.Attributes;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;
using HaemophilusWeb.Validators;
using HaemophilusWeb.Views.Utils;

namespace HaemophilusWeb.ViewModels
{
    [Validator(typeof(MeningoIsolateViewModelValidator))]
    public class MeningoIsolateViewModel : MeningoIsolateBase
    {
        private List<EpsilometerTestViewModel> etests;

        public MeningoIsolateViewModel()
        {
            EpsilometerTestViewModels = new List<EpsilometerTestViewModel>();
            Report = new string[0];
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

        public IEnumerable<Typing> Typings { get; set; }
        

        public string SenderName { get; set; }
        public string SenderStreet { get; set; }
        public string SenderCity { get; set; }

        public string SamplingDate { get; set; }
        public string ReceivingDate { get; set; }
        public string Patient { get; set; }
        public string PatientBirthDate { get; set; }
        public string PatientPostalCode { get; set; }
        public string SenderLaboratoryNumber { get; set; }

        public string EvaluationString { get; set; }

        [Display(Name = "Befund")]
        public string[] Report { get; set; }

        [Display(Name = "Teilbefund")]
        public string InterpretationPreliminary { get; set; }

        [Obsolete("Use Result array instead")]
        [Display(Name = "Befund")]
        public string Interpretation { get; set; }

        [Obsolete("Use Result array instead")]
        public string InterpretationDisclaimer { get; set; }
        public bool HasInterpretationDisclaimer => !string.IsNullOrEmpty(InterpretationDisclaimer);


        public string Date
        {
            get { return DateTime.Now.ToReportFormat(); }
        }

        public bool HasETests => ETests.Any();

        //TODO refactor duplication in IsolateViewModel
        public IEnumerable<EpsilometerTestReportModel> ETests
        {
            get
            {
                return
                    EpsilometerTestViewModels.Where(e => e.EucastClinicalBreakpointId != null && e.Measurement.HasValue)
                        .Select(EpsilometerTestReportModel.CreateFromViewModel);
            }
        }
    }
}