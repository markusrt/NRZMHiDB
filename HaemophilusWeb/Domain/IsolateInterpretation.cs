using System.Collections.Generic;
using HaemophilusWeb.Models;
using HaemophilusWeb.Views.Utils;

namespace HaemophilusWeb.Domain
{
    public static class IsolateInterpretation
    {
        private const string DisclaimerTemplate =
            "Der Nachweis von Haemophilus influenzae aus primär sterilem Material ist nach §7 IfSG meldepflichtig. Meldekategorie dieses Befundes: {0}.";

        private const string TypingNotPossible =
            "Die Ergebnisse sprechen für einen nicht-typisierbaren Haemophilus influenzae (NTHi).";

        private static readonly List<SerotypeAgg> SpecificAgglutination = new List<SerotypeAgg>
        {
            SerotypeAgg.A,
            SerotypeAgg.B,
            SerotypeAgg.C,
            SerotypeAgg.D,
            SerotypeAgg.E,
            SerotypeAgg.F
        };

        public static InterpretationResult Interpret(IsolateBase isolate)
        {
            var serotypePcr = isolate.SerotypePcr;
            var serotypePcrDescription = EnumEditor.GetEnumDescription(serotypePcr);
            var agglutination = isolate.Agglutination;
            var agglutinationDescription = EnumEditor.GetEnumDescription(agglutination);
            var interpretation = "Diskrepante Ergebnisse, bitte Datenbankeinträge kontrollieren.";
            var interpretationDisclaimer = string.Empty;

            if (agglutination == SerotypeAgg.Negative)
            {
                if (isolate.BexA == TestResult.Negative)
                {
                    if (serotypePcr == SerotypePcr.Negative || serotypePcr == SerotypePcr.NotDetermined)
                    {
                        interpretation = TypingNotPossible;
                        interpretationDisclaimer = string.Format(DisclaimerTemplate,
                            "Haemophilus influenzae, unbekapselt");
                    }
                    else if (serotypePcr != SerotypePcr.NotDetermined)
                    {
                        interpretation =
                            string.Format(
                                "{0} Ein vorhandener genetischer Kapsellocus für Polysaccharide des Serotyps {1} wird nicht exprimiert.",
                                TypingNotPossible, serotypePcrDescription);
                        interpretationDisclaimer = string.Format(DisclaimerTemplate,
                            "Haemophilus influenzae, unbekapselt");
                    }
                }
                else if (isolate.BexA == TestResult.NotDetermined && serotypePcr == SerotypePcr.NotDetermined)
                {
                    interpretation =
                        string.Format(
                            "{0} Eine molekularbiologische Typisierung wurde aus epidemiologischen und Kostengründen nicht durchgeführt.",
                            TypingNotPossible);
                }
            }
            if (SpecificAgglutination.Contains(agglutination) && isolate.BexA == TestResult.Positive &&
                (agglutination.ToString() == serotypePcr.ToString() || serotypePcr == SerotypePcr.NotDetermined))
            {
                interpretation =
                    string.Format(
                        "Die Ergebnisse sprechen für eine Infektion mit Haemophilus influenzae des Serotyp {0} (Hi{0}).",
                        agglutinationDescription);
                interpretationDisclaimer = string.Format(DisclaimerTemplate,
                    string.Format("Haemophilus influenzae, Serotyp {0}", agglutinationDescription));
            }
            return new InterpretationResult
            {
                Interpretation = interpretation,
                InterpretationDisclaimer = interpretationDisclaimer
            };
        }
    }

    public class InterpretationResult
    {
        public string Interpretation { get; set; }
        public string InterpretationDisclaimer { get; set; }
    }
}