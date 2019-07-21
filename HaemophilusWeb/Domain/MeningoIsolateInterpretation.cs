using System.Collections.Generic;
using System.IO;
using System.Linq;
using HaemophilusWeb.Migrations;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;
using HaemophilusWeb.ViewModels;
using Newtonsoft.Json;
using SmartFormat;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Settings;

namespace HaemophilusWeb.Domain
{
    public class MeningoIsolateInterpretation
    {
        static MeningoIsolateInterpretation()
        {
            Smart.Default.AddExtensions(new EnumFormatter());
        }

        private static readonly Dictionary<string, StemInterpretationRule> StemInterpretationRules = DeserializeFromResource<Dictionary<string, StemInterpretationRule>>("HaemophilusWeb.Domain.Interpretation.StemRules.json");
        
        private Dictionary<string, string> StemInterpretations = DeserializeFromResource<Dictionary<string, string>>("HaemophilusWeb.Domain.Interpretation.StemInterpretations.json");

        private Dictionary<string, string> StemIdentifications = DeserializeFromResource<Dictionary<string, string>>("HaemophilusWeb.Domain.Interpretation.StemIdentifications.json");
       

        private Dictionary<string, string[]> StemTypings = new Dictionary<string, string[]>
        {
            {"StemInterpretation_02", new[] {"Serogenogroup", "PorA", "FetA"}},
            {"StemInterpretation_03", new[] { "GrowthOnMartinLewisAgar", "GrowthOnBloodAgar"}},
            {"StemInterpretation_04", new[] { "GrowthOnMartinLewisAgar", "GrowthOnBloodAgar", "Onpg", "GammaGt", "MaldiTof"}},
            {"StemInterpretation_05", new[] { "GrowthOnMartinLewisAgar", "GrowthOnBloodAgar", "Onpg", "GammaGt", "MaldiTof"}},
            {"StemInterpretation_06", new[] { "Agglutination"}},
        };

        private Dictionary<string, Typing> TypingTemplates = new Dictionary<string, Typing>
        {
            {
                "Serogenogroup", new Typing
                {
                    Attribute = "Serogenogruppe",
                    Value = "{SerogroupPcr}"
                }
            },
            {
                "PorA", new Typing
                {
                    Attribute = "PorA - Sequenztyp",
                    Value = "{PorAVr1}, {PorAVr2}"
                }
            },
            {
                "FetA", new Typing
                {
                    Attribute = "FetA - Sequenztyp",
                    Value = "{FetAVr}"
                }
            },
            {
                "GrowthOnBloodAgar", new Typing
                {
                    Attribute = "Wachstum auf Blutagar",
                    Value = "{GrowthOnBloodAgar:enum()}"
                }
            },
            {
                "GrowthOnMartinLewisAgar", new Typing
                {
                    Attribute = "Wachstum auf Martin-Lewis-Agar",
                    Value = "{GrowthOnMartinLewisAgar:enum()}"
                }
            },
            {
                "Onpg", new Typing
                {
                    Attribute = "β-Galaktosidase",
                    Value = "{Onpg:enum()}"
                }
            },
            {
                "GammaGt", new Typing
                {
                    Attribute = "γ-Glutamyltransferase",
                    Value = "{GammaGt:enum()}"
                }
            },
            {
                "MaldiTof", new Typing
                {
                    Attribute = "MALDI-TOF (VITEK MS)",
                    Value = "{MaldiTofBestMatch}"
                }
            },
            {
                "Agglutination", new Typing
                {
                    Attribute = "Serogruppe",
                    Value = "{Agglutination:enum()}"
                }
            },
        };

        private readonly List<Typing> typings = new List<Typing>();

        public IEnumerable<Typing> Typings => typings;

        public InterpretationResult Result { get; private set; } = new InterpretationResult();

        public void Interpret(MeningoIsolate isolate)
        {
            typings.Clear();

            Result = new InterpretationResult
            {
                Interpretation = "Interpretation: Diskrepante Ergebnisse, bitte Datenbankeinträge kontrollieren."
            };

            var matchingRule = StemInterpretationRules.FirstOrDefault(r => CheckRule(r.Value, isolate));
            
            if (matchingRule.Key != null)
            {
                Smart.Default.Settings.FormatErrorAction = ErrorAction.ThrowError;
                Smart.Default.Settings.ParseErrorAction = ErrorAction.ThrowError;
                if (!string.IsNullOrEmpty(StemIdentifications[matchingRule.Key]))
                {
                    typings.Add(new Typing {Attribute = "Identifikation", Value = StemIdentifications[matchingRule.Key] });
                }

                var interpretation = StemInterpretations[matchingRule.Key];
                Result.Interpretation = !string.IsNullOrEmpty(interpretation) ? "Interpretation: " + Smart.Format(interpretation, isolate) : string.Empty;

                if (StemTypings.ContainsKey(matchingRule.Key))
                {
                    foreach (var typingTemplateKey in StemTypings[matchingRule.Key])
                    {
                        var template = TypingTemplates[typingTemplateKey];
                        typings.Add(new Typing
                        {
                            Attribute = template.Attribute,
                            Value = Smart.Format(template.Value, isolate)
                        });
                    }
                }
            }
        }

        private static T DeserializeFromResource<T>(string resourceName)
        {
            using (var stream = typeof(MeningoIsolateInterpretation).Assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
            }
        }

        private bool CheckRule(StemInterpretationRule rule, MeningoIsolate isolate)
        {
            return rule.SendingInvasive == isolate.Sending?.Invasive
                && rule.GrowthOnBloodAgar == isolate.GrowthOnBloodAgar
                && rule.GrowthOnMartinLewisAgar == isolate.GrowthOnMartinLewisAgar
                && (!rule.Oxidase.HasValue || rule.Oxidase == isolate.Oxidase)
                && (!rule.Agglutination.HasValue || rule.Agglutination == isolate.Agglutination)
                && (!rule.Onpg.HasValue || rule.Onpg == isolate.Onpg)
                && (!rule.GammaGt.HasValue || rule.GammaGt == isolate.GammaGt)
                && (!rule.SerogroupPcr.HasValue || rule.SerogroupPcr == isolate.SerogroupPcr)
                && (!rule.MaldiTof.HasValue || rule.MaldiTof == isolate.MaldiTof)
                && (!rule.PorAPcr.HasValue || rule.PorAPcr == isolate.PorAPcr)
                && (!rule.FetAPcr.HasValue || rule.FetAPcr == isolate.FetAPcr);
        }
    }

    

    class StemInterpretationRule
    {
        public YesNo? SendingInvasive { get; set; }
        public Growth GrowthOnBloodAgar { get; set; }
        public Growth GrowthOnMartinLewisAgar { get; set; }
        public TestResult? Oxidase { get; set; }
        public MeningoSerogroupAgg? Agglutination { get; set; }
        public TestResult? Onpg { get; set; }
        public TestResult? GammaGt { get; set;  }
        public MeningoSerogroupPcr? SerogroupPcr { get; set; }
        public UnspecificTestResult? MaldiTof { get; set; }
        public NativeMaterialTestResult? PorAPcr { get; set; }
        public NativeMaterialTestResult? FetAPcr { get; set; }
    }

    public class EnumFormatter : IFormatter
    {
        public string[] Names { get; set; } = { "enum" };

        public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            var currentValue = formattingInfo.CurrentValue;
            var type = formattingInfo.CurrentValue.GetType();
            var iCanHandleThisInput = type.IsEnum;
            if (!iCanHandleThisInput)
                return false;

            formattingInfo.Write(EnumUtils.GetEnumDescription(type, currentValue));

            return true;
        }
    }
}