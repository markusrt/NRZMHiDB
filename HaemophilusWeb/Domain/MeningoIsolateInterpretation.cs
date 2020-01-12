using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HaemophilusWeb.Migrations;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;
using HaemophilusWeb.ViewModels;
using Newtonsoft.Json;
using SmartFormat;
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
        private static readonly Dictionary<string, NativeMaterialInterpretationRule> NativeMaterialInterpretationRules = DeserializeFromResource<Dictionary<string, NativeMaterialInterpretationRule>>("HaemophilusWeb.Domain.Interpretation.NativeMaterialRules.json");

        private static readonly Dictionary<string, Typing> TypingTemplates = DeserializeFromResource<Dictionary<string, Typing>>("HaemophilusWeb.Domain.Interpretation.TypingTemplates.json");

        private readonly List<Typing> typings = new List<Typing>();

        public IEnumerable<Typing> Typings => typings;

        public InterpretationResult Result { get; private set; } = new InterpretationResult();

        public void Interpret(MeningoIsolate isolate)
        {
            typings.Clear();

            Result = new InterpretationResult
            {
                Interpretation = "Diskrepante Ergebnisse, bitte Datenbankeinträge kontrollieren."
            };

            if (isolate.Sending.Material == MeningoMaterial.NativeMaterial)
            {
                RunNativeMaterialInterpretation(isolate);
            }
            else
            {
                RunStemInterpretation(isolate);
            }
        }

        private void RunNativeMaterialInterpretation(MeningoIsolate isolate)
        {
            var matchingRule = NativeMaterialInterpretationRules.FirstOrDefault(r => CheckNativeMaterialRule(r.Value, isolate));

            if (matchingRule.Key != null)
            {
                var rule = matchingRule.Value;
                Smart.Default.Settings.FormatErrorAction = ErrorAction.ThrowError;
                Smart.Default.Settings.ParseErrorAction = ErrorAction.ThrowError;

                var interpretation = rule.Interpretation;
                Result.Interpretation = !string.IsNullOrEmpty(interpretation)
                    ? Smart.Format(interpretation, isolate)
                    : string.Empty;

                Result.InterpretationDisclaimer = rule.InterpretationDisclaimer != null
                    ? Smart.Format(rule.InterpretationDisclaimer, isolate)
                    : string.Empty;

                foreach (var typingTemplateKey in rule.Typings)
                {
                    var template = TypingTemplates[typingTemplateKey];
                    typings.Add(new Typing
                    {
                        Attribute = template.Attribute,
                        Value = Smart.Format(template.Value, isolate, rule)
                    });
                }
            }
        }

        private void RunStemInterpretation(MeningoIsolate isolate)
        {
            var matchingRule = StemInterpretationRules.FirstOrDefault(r => CheckStemRule(r.Value, isolate));

            if (matchingRule.Key != null)
            {
                var rule = matchingRule.Value;
                Smart.Default.Settings.FormatErrorAction = ErrorAction.ThrowError;
                Smart.Default.Settings.ParseErrorAction = ErrorAction.ThrowError;
                if (!string.IsNullOrEmpty(rule.Identification))
                {
                    typings.Add(new Typing {Attribute = "Identifikation", Value = rule.Identification});
                }

                var interpretation = rule.Interpretation;
                //TODO Add InterpretationDisclaimer
                Result.Interpretation = !string.IsNullOrEmpty(interpretation)
                    ? Smart.Format(interpretation, isolate)
                    : string.Empty;

                foreach (var typingTemplateKey in rule.Typings)
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

        private static T DeserializeFromResource<T>(string resourceName)
        {
            using (var stream = typeof(MeningoIsolateInterpretation).Assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
            }
        }

        private bool CheckNativeMaterialRule(NativeMaterialInterpretationRule rule, MeningoIsolate isolate)
        {
            return rule.CsbPcr.Contains(isolate.CsbPcr)
                   && rule.CscPcr.Contains(isolate.CscPcr)
                   && rule.CswyPcr.Contains(isolate.CswyPcr)
                   && rule.CswyAllele == isolate.CswyAllele
                   && rule.PorAPcr.Contains(isolate.PorAPcr)
                   && rule.FetAPcr.Contains(isolate.FetAPcr)
                   && rule.RibosomalRna16S.Contains(isolate.RibosomalRna16S)
                   && (rule.RibosomalRna16SBestMatch == null || rule.RibosomalRna16SBestMatch == isolate.RibosomalRna16SBestMatch)
                   && rule.RealTimePcr.Contains(isolate.RealTimePcr)
                   && rule.RealTimePcrResult == isolate.RealTimePcrResult;
        }

        private bool CheckStemRule(StemInterpretationRule rule, MeningoIsolate isolate)
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
}