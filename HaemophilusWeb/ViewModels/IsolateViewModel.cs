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

        [Display(Name = "Entnahmeort")]
        public string SamplingLocation { get; set; }

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
                var properties = GetType().GetProperties();
                foreach (
                    var typingProperty in
                        properties.Where(p => p.PropertyType == typeof (TestResult) && p.Name != "BetaLactamase"))
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
                if (SerotypePcr != Serotype.NotDetermined)
                {
                    yield return
                        new Typing {Attribute = "Kapselgenotypen", Value = EnumEditor.GetEnumDescription(SerotypePcr)};
                }
                if (FactorTest != FactorTest.NotDetermined)
                {
                    yield return
                        new Typing {Attribute = "Faktoren-test", Value = EnumEditor.GetEnumDescription(FactorTest)};
                }
                if (ApiNh == UnspecificTestResult.Determined)
                {
                    yield return
                        new Typing
                        {
                            Attribute = "api NH",
                            Value = string.Format("{0}, {1}%", ApiNhBestMatch, DoubleToString(ApiNhMatchInPercent))
                        };
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
                            Value = string.Format("{0} {1}", FtsiEvaluation1, FtsiEvaluation2)
                        };
                }
                if (Mlst == UnspecificOrNoTestResult.Determined)
                {
                    yield return
                        new Typing {Attribute = "MLST", Value = MlstSequenceType};
                }
            }
        }

        public string SamplingDate { get; set; }
        public string ReceivingDate { get; set; }
        public string Patient { get; set; }
        public string PatientBirthDate { get; set; }
        public string SenderLaboratoryNumber { get; set; }
        public string EvaluationString { get; set; }
        public string Interpretation { get; set; }
        public string InterpretationDisclaimer { get; set; }

        public string Date
        {
            get { return DateTime.Now.ToReportFormat(); }
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

        public string BetalactamaseString
        {
            get { return EnumEditor.GetEnumDescription(BetaLactamase); }
        }

        private static string GetDisplayName(MemberInfo member)
        {
            var displayAttribute = member.GetCustomAttribute<DisplayAttribute>();
            return displayAttribute == null ? member.Name : displayAttribute.Name;
        }

        private EpsilometerTestReportModel CreateEpsilometerTestReportModel(
            EpsilometerTestViewModel epsilometerTestViewModel)
        {
            var reportModel = new EpsilometerTestReportModel
            {
                Antibiotic = EnumEditor.GetEnumDescription(epsilometerTestViewModel.Antibiotic),
                Measurement = FloatToString(epsilometerTestViewModel.Measurement),
                Result = EnumEditor.GetEnumDescription(epsilometerTestViewModel.Result),
                MicBreakpointResistent = FloatToString(epsilometerTestViewModel.MicBreakpointResistent),
                MicBreakpointSusceptible = FloatToString(epsilometerTestViewModel.MicBreakpointSusceptible),
                ValidFromYear = epsilometerTestViewModel.ValidFromYear,
            };
            return reportModel;
        }

        private static string FloatToString(float value)
        {
            return Math.Round(value, 3).ToString(CultureInfo.CurrentCulture);
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

    public class Typing
    {
        public string Attribute { get; set; }
        public string Value { get; set; }
    }

    public class EpsilometerTestReportModel
    {
        public string Antibiotic { get; set; }
        public string Result { get; set; }
        public string MicBreakpointSusceptible { get; set; }
        public string MicBreakpointResistent { get; set; }
        public string Measurement { get; set; }
        public int ValidFromYear { get; set; }
    }
}