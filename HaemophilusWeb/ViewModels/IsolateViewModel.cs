using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using FluentValidation.Attributes;
using HaemophilusWeb.Models;
using HaemophilusWeb.Utils;
using HaemophilusWeb.Validators;
using HaemophilusWeb.Views.Utils;

namespace HaemophilusWeb.ViewModels
{
    [Validator(typeof (IsolateViewModelValidator))]
    public class IsolateViewModel : IsolateBase
    {
        private List<EpsilometerTestViewModel> etests;

        public IsolateViewModel()
        {
            EpsilometerTestViewModels = new List<EpsilometerTestViewModel>();
        }

        [Display(Name = "Invasiv")]
        public string Invasive { get; set; }

        [Display(Name = "Entnahmeort")]
        public string SamplingLocation { get; set; }

        [Display(Name = "Material")]
        public string Material { get; set; }

        [Display(Name = "Patientenalter bei Entnahme")]
        public int PatientAgeAtSampling { get; set; }

        public IEnumerable<Typing> Typings
        {
            get
            {
                var properties = GetType().GetProperties();
                foreach (
                    var typingProperty in
                        properties.Where(p => p.PropertyType == typeof (TestResult) && p.Name != "BetaLactamase"
                                              && p.Name != "Oxidase"))
                {
                    var value = (TestResult) typingProperty.GetValue(this);
                    var name = GetDisplayName(typingProperty);
                    if (value != TestResult.NotDetermined)
                    {
                        yield return new Typing {Attribute = name, Value = EnumEditor.GetEnumDescription(value)};
                    }
                }
                if (OuterMembraneProteinP6 != SpecificTestResult.NotDetermined)
                {
                    yield return
                        new Typing {Attribute = "ompP6", Value = EnumEditor.GetEnumDescription(OuterMembraneProteinP6)};
                }
                if (SerotypePcr != SerotypePcr.NotDetermined)
                {
                    yield return
                        new Typing {Attribute = "Kapselgenotypen", Value = EnumEditor.GetEnumDescription(SerotypePcr)};
                }
                if (RibosomalRna16S == UnspecificTestResult.Determined)
                {
                    yield return
                        new Typing
                        {
                            Attribute = "16S rRNA",
                            Value =
                                string.Format("{0}, {1}%", RibosomalRna16SBestMatch,
                                    DoubleToString(RibosomalRna16SMatchInPercent))
                        };
                }
                if (MaldiTof == UnspecificTestResult.Determined)
                {
                    yield return
                        new Typing
                        {
                            Attribute = "MALDI-TOF",
                            Value =
                                string.Format("{0}, {1}", MaldiTofBestMatch,
                                    DoubleToString(MaldiTofMatchConfidence))
                        };
                }
                if (Ftsi == UnspecificTestResult.Determined)
                {
                    yield return
                        new Typing
                        {
                            Attribute = "ftsI",
                            Value = string.Format("{0} {1} {2}", FtsiEvaluation1, FtsiEvaluation2, FtsiEvaluation3)
                        };
                }
                if (Mlst == UnspecificOrNoTestResult.Determined)
                {
                    yield return
                        new Typing {Attribute = "MLST", Value = MlstSequenceType};
                }
            }
        }

        public string SenderName { get; set; }
        public string SenderDepartment { get; set; }
        public string SenderStreet { get; set; }
        public string SenderCity { get; set; }

        public string SamplingDate { get; set; }
        public string ReceivingDate { get; set; }

        [Display(Name = "Patienten-Nr.")]
        public int PatientId { get; set; }

        public string Patient { get; set; }
        public string PatientBirthDate { get; set; }
        public string PatientPostalCode { get; set; }
        public string SenderLaboratoryNumber { get; set; }
        public string EvaluationString { get; set; }

        [Display(Name = "Befund")]
        public string Interpretation { get; set; }

        [Display(Name = "Teilbefund")]
        public string InterpretationPreliminary { get; set; }
        public string InterpretationDisclaimer { get; set; }

        public string Comment { get; set; }
        public bool HasComment => !string.IsNullOrEmpty(Comment);

        public string Announcement { get; set; }
        public bool HasAnnouncement => !string.IsNullOrEmpty(Announcement);

        public bool HasCommentOrAnnouncement => HasComment || HasAnnouncement;

        public string DemisIdQrImageUrl { get; set; }

        public string Date
        {
            get { return DateTime.Now.ToReportFormat(); }
        }

        public ICollection<EpsilometerTestViewModel> EpsilometerTestViewModels { get; set; }

        public IEnumerable<EpsilometerTestReportModel> ETests
        {
            get
            {
                return
                    EpsilometerTestViewModels.Where(e => e.EucastClinicalBreakpointId != null && e.Measurement.HasValue)
                        .Select(EpsilometerTestReportModel.CreateFromViewModel);
            }
        }

        public string AgglutinationString
        {
            get { return EnumEditor.GetEnumDescription(Agglutination); }
        }

        public string BetalactamaseString
        {
            get { return EnumEditor.GetEnumDescription(BetaLactamase); }
        }

        private static string GetDisplayName(MemberInfo member)
        {
            var displayAttribute = member.GetCustomAttribute<DisplayAttribute>();
            return displayAttribute == null ? member.Name : displayAttribute.Name;
        }

        private static string DoubleToString(double? value)
        {
            return value.HasValue ? DoubleToString(value.Value) : string.Empty;
        }

        private static string DoubleToString(double value)
        {
            return Math.Round(value, 3).ToString(CultureInfo.CurrentCulture);
        }
    }
}