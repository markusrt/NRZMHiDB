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

        public IEnumerable<Typing> Typings
        {
            get
            {
                if (SerogroupPcr != MeningoSerogroupPcr.NotDetermined && Agglutination == MeningoSerogroupAgg.NotDetermined)
                {
                    yield return new Typing
                    {
                        Attribute = "Serogenogruppe",
                        Value = EnumUtils.GetEnumDescription<MeningoSerogroupPcr>(SerogroupPcr)
                    };
                }
                if (Agglutination != MeningoSerogroupAgg.NotDetermined)
                {
                    var value = EnumUtils.GetEnumDescription<MeningoSerogroupAgg>(Agglutination);
                    if (Agglutination == MeningoSerogroupAgg.Auto)
                    {
                        value = "Autoagglutination";
                    }
                    else if (Agglutination == MeningoSerogroupAgg.Poly)
                    {
                        value = "Polyagglutination";
                    }
                    yield return new Typing
                    {
                        Attribute = "Serogruppe",
                        Value = value
                    };
                    if ((Agglutination == MeningoSerogroupAgg.Auto || Agglutination == MeningoSerogroupAgg.Poly) && SerogroupPcr != MeningoSerogroupPcr.NotDetermined)
                    {
                        yield return new Typing
                        {
                            Attribute = "Neisseria meningitidis",
                            Value = $"Serogenogruppe {EnumUtils.GetEnumDescription<MeningoSerogroupPcr>(SerogroupPcr)}"
                        };
                    }
                }
                if (PorAPcr == NativeMaterialTestResult.Positive)
                {
                    yield return new Typing
                    {
                        Attribute = "PorA-Sequenztyp",
                        Value = $"{PorAVr1}, {PorAVr2}"
                    };
                }
                if (FetAPcr == NativeMaterialTestResult.Positive)
                {
                    yield return new Typing
                    {
                        Attribute = "FetA-Sequenztyp",
                        Value = $"{FetAVr}"
                    };
                }
                if (GrowthOnMartinLewisAgar == Growth.No && GrowthOnBloodAgar == Growth.ATypicalGrowth)
                {
                    yield return new Typing
                    {
                        Attribute = "Wachstum auf Martin-Lewis-Agar",
                        Value = EnumUtils.GetEnumDescription<Growth>(GrowthOnMartinLewisAgar)
                    };
                    yield return new Typing
                    {
                        Attribute = "Wachstum auf Blutagar",
                        Value = EnumUtils.GetEnumDescription<Growth>(GrowthOnBloodAgar)
                    };
                    if (Onpg != TestResult.NotDetermined)
                    {
                        yield return new Typing
                        {
                            Attribute = "β-Galaktosidase",
                            Value = EnumUtils.GetEnumDescription<TestResult>(Onpg)
                        };
                    }
                    if (GammaGt != TestResult.NotDetermined)
                    {
                        yield return new Typing
                        {
                            Attribute = "γ-Glutamyltransferase",
                            Value = EnumUtils.GetEnumDescription<TestResult>(GammaGt)
                        };
                    }
                    if (MaldiTof != UnspecificTestResult.NotDetermined)
                    {
                        yield return new Typing
                        {
                            Attribute = "MALDI-TOF (VITEK MS)",
                            Value = MaldiTofBestMatch
                        };
                    }
                }
                if (GrowthOnBloodAgar == Growth.No)
                {
                    yield return new Typing
                    {
                        Attribute = "Wachstum auf Martin-Lewis-Agar",
                        Value = EnumUtils.GetEnumDescription<Growth>(GrowthOnMartinLewisAgar)
                    };
                }
            }
        }

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
        public string Interpretation { get; set; }

        [Display(Name = "Teilbefund")]
        public string InterpretationPreliminary { get; set; }
        public string InterpretationDisclaimer { get; set; }

        public string Date
        {
            get { return DateTime.Now.ToReportFormat(); }
        }

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