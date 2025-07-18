﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using HaemophilusWeb.Utils;
using HaemophilusWeb.ViewModels;
using Newtonsoft.Json;
using SmartFormat.Core.Settings;
using SmartFormat;

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

        public string Serogroup { get; set; }

        public InterpretationResult Result { get; private set; } = new InterpretationResult();
        public string Rule { get; set; }
        
        public bool NoMeningococci { get; set; }

        public void Interpret(MeningoIsolate isolate)
        {
            //TODO fix mix of model and business logic in that class it  could just return
            //     an interpretation here whereas this class would be more the interpreter.
            typings.Clear();
            Serogroup = null;
            Rule = null;
            NoMeningococci = false;

            Result.Report = new [] { "Diskrepante Ergebnisse, bitte Datenbankeinträge kontrollieren." };
            Smart.Default.Settings.Formatter.ErrorAction = FormatErrorAction.ThrowError;
            Smart.Default.Settings.Parser.ErrorAction = ParseErrorAction.ThrowError;

            if (isolate.Sending.Material.IsNativeMaterial())
            {
                RunNativeMaterialInterpretation(isolate);
            }
            else
            {
                RunStemInterpretation(isolate);
            }

            DetectSerogroup();
        }

        private void DetectSerogroup()
        {
            var serogroup = Typings.SingleOrDefault(t => t.Attribute == "Serogruppe")?.Value;
            var serogenogroup = Typings.SingleOrDefault(t => t.Attribute == "Serogenogruppe")?.Value;
            
            if (serogroup != null && serogenogroup == null)
            {
                Serogroup = serogroup;
            }
            if (serogenogroup != null && serogroup == null)
            {
                Serogroup = serogenogroup;
            }
            if (serogenogroup != null && serogroup != null)
            {
                if (serogenogroup == serogroup)
                {
                    Serogroup = serogenogroup;
                }
                else if(serogroup.Contains("deutet auf unbekapselte Meningokokken hin") || serogroup.Contains("Nicht-invasiv"))
                {
                    Serogroup = serogenogroup;
                }
            }

            var molecularTyping = Typings.SingleOrDefault(t => t.Attribute == "Molekulare Typisierung")?.Value;
            if (molecularTyping != null)
            {
                var match = Regex.Match(molecularTyping, "Das Serogruppe.?-(.*)-spezifische .*-Gen wurde nachgewiesen.");
                if (match.Success)
                {
                    Serogroup = match.Groups[1].Value;
                }
            }
            
            if (Serogroup != null)
            {
                if (Serogroup == "nicht gruppierbar" || Serogroup.Contains("Poly") || Serogroup.Contains("Auto"))
                {
                    Serogroup = "NG";
                }
                else if (Serogroup.Contains("cnl"))
                {
                    Serogroup = "cnl";
                }
                Serogroup = Regex.Replace(Serogroup, ".\\(.*\\)", "");
            }

            var agglutination = Typings.SingleOrDefault(t => t.Attribute == "Agglutination")?.Value;
            if (Serogroup == null && agglutination != null && agglutination.Contains("keine Agglutination"))
            {
                Serogroup = "NG";
            }
        }

        private void RunNativeMaterialInterpretation(MeningoIsolate isolate)
        {
            var matchingRule = NativeMaterialInterpretationRules.FirstOrDefault(r => CheckNativeMaterialRule(r.Value, isolate));

            if (matchingRule.Key != null)
            {
                var rule = matchingRule.Value;
                Rule = matchingRule.Key;
                NoMeningococci = rule.NoMeningococci;

                Result.Comment = rule.Comment;
                Result.Report = rule.Report.Select(r => Smart.Format(r, isolate, rule)).ToArray();

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
            var matchingRule = StemInterpretationRules.FirstOrDefault(r => CheckStemRule(r.Key, r.Value, isolate));

            if (matchingRule.Key != null)
            {
                var rule = matchingRule.Value;
                Rule = matchingRule.Key;
                NoMeningococci = rule.NoMeningococci;

                if (!string.IsNullOrEmpty(rule.Identification))
                {
                    typings.Add(new Typing {Attribute = "Identifikation", Value = rule.Identification});
                }

                Result.Comment = rule.Comment;
                Result.Report = rule.Report.Select(r => Smart.Format(r, isolate, rule)).ToArray();
                
                foreach (var typingTemplateKey in rule.Typings)
                {
                    var templateKey = AdaptTemplateKeyForMaldiTofSpecialCase(isolate, typingTemplateKey);
                    var template = TypingTemplates[templateKey];
                    typings.Add(new Typing
                    {
                        Attribute = template.Attribute,
                        Value = Smart.Format(template.Value, isolate)
                    });
                }
            }
        }

        private static string AdaptTemplateKeyForMaldiTofSpecialCase(MeningoIsolate isolate, string typingTemplateKey)
        {
            var templateKey = typingTemplateKey;
            if (templateKey == "MaldiTof")
            {
                if (isolate.MaldiTofBiotyper == UnspecificTestResult.Determined)
                {
                    templateKey = "MaldiTofBiotyper";
                }
                else
                {
                    templateKey = "MaldiTofVitek";
                }
            }

            return templateKey;
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

        private bool CheckStemRule(string argKey, StemInterpretationRule rule, MeningoIsolate isolate)
        {
            var checkStemRule = (rule.SendingInvasive == null || rule.SendingInvasive.Contains(isolate.Sending?.Invasive))
                                && rule.GrowthOnBloodAgar == isolate.GrowthOnBloodAgar
                                && (rule.GrowthOnMartinLewisAgar == null || rule.GrowthOnMartinLewisAgar.Contains(isolate.GrowthOnMartinLewisAgar))
                                && (!rule.Oxidase.HasValue || rule.Oxidase == isolate.Oxidase)
                                && (rule.Agglutination == null || rule.Agglutination.Contains(isolate.Agglutination))
                                && (!rule.Onpg.HasValue || rule.Onpg == isolate.Onpg)
                                && (rule.GammaGt == null || rule.GammaGt.Contains(isolate.GammaGt))
                                && (rule.SerogroupPcr == null || rule.SerogroupPcr.Contains(isolate.SerogroupPcr))
                                && (!rule.MaldiTof.HasValue 
                                    || ( rule.MaldiTof == UnspecificTestResult.NotDetermined && rule.MaldiTof == isolate.MaldiTofVitek && rule.MaldiTof == isolate.MaldiTofBiotyper )
                                    || ( rule.MaldiTof == UnspecificTestResult.Determined && (rule.MaldiTof == isolate.MaldiTofVitek || rule.MaldiTof == isolate.MaldiTofBiotyper))
                                    )
                                && (!rule.PorAPcr.HasValue || rule.PorAPcr == isolate.PorAPcr)
                                && (!rule.FetAPcr.HasValue || rule.FetAPcr == isolate.FetAPcr);
            return checkStemRule;
        }
    }
}