using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HaemophilusWeb.Models;
using HaemophilusWeb.Utils;
using HaemophilusWeb.ViewModels;
using HaemophilusWeb.Views.Utils;
using Newtonsoft.Json;
using SmartFormat.Core.Settings;
using SmartFormat;

namespace HaemophilusWeb.Domain
{
    public class IsolateInterpretation
    {
        static IsolateInterpretation()
        {
            Smart.Default.AddExtensions(new EnumFormatter());
        }

        private static readonly Dictionary<string, StemInterpretationRule> StemInterpretationRules = DeserializeFromResource<Dictionary<string, StemInterpretationRule>>("HaemophilusWeb.Domain.Interpretation.StemRules.json");
        private static readonly Dictionary<string, NativeMaterialInterpretationRule> NativeMaterialInterpretationRules = DeserializeFromResource<Dictionary<string, NativeMaterialInterpretationRule>>("HaemophilusWeb.Domain.Interpretation.NativeMaterialRules.json");
        private static readonly Dictionary<string, Typing> TypingTemplates = DeserializeFromResource<Dictionary<string, Typing>>("HaemophilusWeb.Domain.Interpretation.TypingTemplates.json");

        private readonly List<Typing> typings = new List<Typing>();

        public IEnumerable<Typing> Typings => typings;
        public string Rule { get; set; }
        private const string NonInvasiveDisclaimer =
            "Eine molekularbiologische Typisierung und Resistenztestungen werden bei nicht-invasiven Isolaten aus epidemiologischen und Kostengründen nicht durchgeführt.";

        private const string InvasiveDisclaimerTemplate =
            "Der direkte Nachweis von Haemophilus influenzae aus Blut oder Liquor ist nach §7 IfSG meldepflichtig. Meldekategorie dieses Befundes: {0}.";

        private const string TypingNotPossiblePlural =
            "Die Ergebnisse sprechen für einen unbekapselten Haemophilus influenzae (sog. \"nicht-typisierbarer\" H. influenzae, NTHi).";

        private const string TypingNotPossibleSingular =
            "Das Ergebnis spricht für einen unbekapselten Haemophilus influenzae (sog. \"nicht-typisierbarer\" H. influenzae, NTHi).";

        private const string TypingNotPossiblePlural2 =
            "Die Ergebnisse sprechen für einen phänotpischen nicht-typisierbaren Haemophilus influenzae (NTHi).";

        private static readonly List<SerotypeAgg> SpecificAgglutination = new List<SerotypeAgg>
        {
            SerotypeAgg.A,
            SerotypeAgg.B,
            SerotypeAgg.C,
            SerotypeAgg.D,
            SerotypeAgg.E,
            SerotypeAgg.F
        };

        public InterpretationResult Interpret(Isolate isolate)
        {
            // Clear previous interpretation state
            typings.Clear();
            Rule = null;

            // Try rule-based interpretation first
            var ruleBasedResult = TryRuleBasedInterpretation(isolate);
            if (ruleBasedResult != null)
            {
                return ruleBasedResult;
            }

            // Fall back to original hardcoded interpretation logic
            var serotypePcr = isolate.SerotypePcr;
            var serotypePcrDescription = EnumEditor.GetEnumDescription(serotypePcr);
            var agglutination = isolate.Agglutination;
            var agglutinationDescription = EnumEditor.GetEnumDescription(agglutination);
            var bexA = isolate.BexA;
            var growth = isolate.Growth;
            var interpretation = "Diskrepante Ergebnisse, bitte Datenbankeinträge kontrollieren.";
            var interpretationPreliminary = "Diskrepante Ergebnisse, bitte Datenbankeinträge kontrollieren.";
            var interpretationDisclaimer = string.Empty;

            if (agglutination == SerotypeAgg.Negative)
            {
                if (bexA == TestResult.Negative)
                {
                    if (serotypePcr == SerotypePcr.Negative || serotypePcr == SerotypePcr.NotDetermined)
                    {
                        interpretation = TypingNotPossiblePlural;
                    }
                    else if (serotypePcr != SerotypePcr.NotDetermined)
                    {
                        interpretation =
                            $"{TypingNotPossiblePlural2} Ein vorhandener genetischer Kapsellocus für Polysaccharide des Serotyps {serotypePcrDescription} wird nicht exprimiert.";
                    }
                }
                else if (bexA == TestResult.NotDetermined && serotypePcr == SerotypePcr.NotDetermined)
                {
                    interpretationPreliminary = TypingNotPossibleSingular;
                    interpretation =
                        $"{TypingNotPossibleSingular} Eine molekularbiologische Typisierung wurde aus epidemiologischen und Kostengründen nicht durchgeführt.";
                }

                interpretationDisclaimer = isolate.Sending.Invasive == YesNo.Yes ? string.Format(InvasiveDisclaimerTemplate, "Haemophilus influenzae, unbekapselt") : NonInvasiveDisclaimer;
            }
            if (SpecificAgglutination.Contains(agglutination) && (bexA == TestResult.Positive || bexA == TestResult.NotDetermined) &&
                (agglutination.ToString() == serotypePcr.ToString() || serotypePcr == SerotypePcr.NotDetermined))
            {
                if (serotypePcr == SerotypePcr.NotDetermined && bexA == TestResult.NotDetermined)
                {
                    interpretationPreliminary =
                        $"Das Ergebnis spricht für eine Infektion mit Haemophilus influenzae des Serotyp {agglutinationDescription} (Hi{agglutinationDescription}).";
                }
                if (bexA == TestResult.Positive)
                {
                    interpretation =
                        $"Die Ergebnisse sprechen für eine Infektion mit Haemophilus influenzae des Serotyp {agglutinationDescription} (Hi{agglutinationDescription}).";
                }

                interpretationDisclaimer = isolate.Sending.Invasive == YesNo.Yes
                    ? string.Format(InvasiveDisclaimerTemplate,
                        $"Haemophilus influenzae, Serotyp {agglutinationDescription}")
                    : NonInvasiveDisclaimer;
            }

            if (agglutination == SerotypeAgg.NotDetermined && serotypePcr == SerotypePcr.NotDetermined && bexA == TestResult.NotDetermined && growth != YesNoOptional.NotStated)
            {
                if (growth == YesNoOptional.Yes)
                {
                    interpretation = "Der eingesendete Stamm konnte nicht angezüchtet werden. Um Wiedereinsendung wird gebeten.";
                    interpretationDisclaimer = "Eine telefonische Vorabmitteilung ist erfolgt.";
                }
                else if (growth == YesNoOptional.No)
                {
                    var evaluation = EnumEditor.GetEnumDescription(isolate.Evaluation);
                    interpretation = "Kein Nachweis von Haemophilus influenzae.";
                    interpretationDisclaimer = $"Beim eingesendeten Isolat handelt es sich am ehesten um {evaluation}.";
                }
            }
            return new InterpretationResult
            {
                Report = [interpretation],
                Interpretation = interpretation,
                InterpretationPreliminary = interpretationPreliminary,
                InterpretationDisclaimer = interpretationDisclaimer,
                OldResult = true
            };
        }

        private InterpretationResult TryRuleBasedInterpretation(Isolate isolate)
        {
            Smart.Default.Settings.Formatter.ErrorAction = FormatErrorAction.ThrowError;
            Smart.Default.Settings.Parser.ErrorAction = ParseErrorAction.ThrowError;

            if (isolate.Sending?.Material == Material.NativeMaterial || isolate.Sending?.Material == Material.IsolatedDna)
            {
                return RunNativeMaterialInterpretation(isolate);
            }
            else
            {
                return RunStemInterpretation(isolate);
            }
        }

        private InterpretationResult RunNativeMaterialInterpretation(Isolate isolate)
        {
            var matchingRule = NativeMaterialInterpretationRules.FirstOrDefault(r => CheckNativeMaterialRule(r.Value, isolate));

            if (matchingRule.Key != null)
            {
                var rule = matchingRule.Value;
                Rule = matchingRule.Key;

                var result = new InterpretationResult
                {
                    Comment = rule.Comment,
                    Report = rule.Report.Select(r => Smart.Format(r, isolate, rule)).ToArray(),
                    Remark = rule.Remark
                };

                foreach (var typingTemplateKey in rule.Typings)
                {
                    var template = TypingTemplates[typingTemplateKey];
                    typings.Add(new Typing
                    {
                        Attribute = template.Attribute,
                        Value = Smart.Format(template.Value, isolate, rule)
                    });
                }

                return result;
            }

            return null;
        }

        private InterpretationResult RunStemInterpretation(Isolate isolate)
        {
            var matchingRule = StemInterpretationRules.FirstOrDefault(r => CheckStemRule(r.Key, r.Value, isolate));

            if (matchingRule.Key != null)
            {
                var rule = matchingRule.Value;
                Rule = matchingRule.Key;

                var result = new InterpretationResult
                {
                    Comment = rule.Comment,
                    Report = rule.Report.Select(r => Smart.Format(r, isolate, rule)).ToArray()
                };

                if (!string.IsNullOrEmpty(rule.Identification))
                {
                    typings.Add(new Typing { Attribute = "Identifikation", Value = rule.Identification });
                }

                foreach (var typingTemplateKey in rule.Typings)
                {
                    var template = TypingTemplates[typingTemplateKey];
                    typings.Add(new Typing
                    {
                        Attribute = template.Attribute,
                        Value = Smart.Format(template.Value, isolate)
                    });
                }

                return result;
            }

            return null;
        }

        private static T DeserializeFromResource<T>(string resourceName)
        {
            using (var stream = typeof(IsolateInterpretation).Assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    return default(T);
                }
                using (var reader = new StreamReader(stream))
                {
                    return JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
                }
            }
        }

        private bool CheckNativeMaterialRule(NativeMaterialInterpretationRule rule, Isolate isolate)
        {
            return rule.RibosomalRna16S.Contains(isolate.RibosomalRna16S)
                   && (rule.RibosomalRna16SBestMatch == null || rule.RibosomalRna16SBestMatch == isolate.RibosomalRna16SBestMatch)
                   && rule.RealTimePcr.Contains(isolate.RealTimePcr)
                   && rule.RealTimePcrResult == isolate.RealTimePcrResult
                   && (!rule.FuculoKinase.HasValue || rule.FuculoKinase == isolate.FuculoKinase);
        }

        private bool CheckStemRule(string argKey, StemInterpretationRule rule, Isolate isolate)
        {
            return (rule.SendingInvasive == null || rule.SendingInvasive.Contains(isolate.Sending?.Invasive))
                   && (!rule.Oxidase.HasValue || rule.Oxidase == isolate.Oxidase)
                   && (!rule.Agglutination.HasValue || rule.Agglutination == isolate.Agglutination)
                   && (!rule.SerotypePcr.HasValue || rule.SerotypePcr == isolate.SerotypePcr)
                   && (!rule.BexA.HasValue || rule.BexA == isolate.BexA)
                   && (!rule.BetaLactamase.HasValue || rule.BetaLactamase == isolate.BetaLactamase)
                   && (!rule.MaldiTofVitek.HasValue || rule.MaldiTofVitek == isolate.MaldiTofVitek)
                   && (!rule.MaldiTofBiotyper.HasValue || rule.MaldiTofBiotyper == isolate.MaldiTofBiotyper)
                   && (!rule.ApiNh.HasValue || rule.ApiNh == isolate.ApiNh)
                   && (!rule.Growth.HasValue || rule.Growth == isolate.Growth)
                   && (!rule.Evaluation.HasValue || rule.Evaluation == isolate.Evaluation);
        }
    }

    public class InterpretationResult
    {
        [Obsolete("Use Result array instead")]
        public string Interpretation { get; set; }

        public string[] Report { get; set; }

        public string InterpretationPreliminary { get; set; }

        [Obsolete("Use Result array instead")]
        public string InterpretationDisclaimer { get; set; }

        [Obsolete("Use new result")]
        public bool OldResult { get; set; }

        public string Comment { get; set; }
        
        public string Remark { get; set; }
    }
}