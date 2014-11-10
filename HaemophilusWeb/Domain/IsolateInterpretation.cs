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

        public static InterpretationResult Interpret(IsolateBase isolate)
        {
            var serotypePcr = isolate.SerotypePcr;
            var agglutionation = isolate.Agglutination;
            var serotypePcrDescription = EnumEditor.GetEnumDescription(serotypePcr);
            var interpretation = "Diskrepante Ergebnisse, bitte Datenbankeinträge kontrollieren.";
            var interpretationDisclaimer = string.Empty;

            if (agglutionation == SerotypeAgg.Negative)
            {
                if (isolate.BexA == TestResult.Negative)
                {
                    if (serotypePcr == SerotypePcr.Negative)
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
            if (agglutionation.ToString() == serotypePcr.ToString() &&
                isolate.BexA == TestResult.Positive)
            {
                interpretation =
                    string.Format(
                        "Die Ergebnisse sprechen für eine Infektion mit Haemophilus influenzae des Serotyp {0} (Hi{0}).",
                        serotypePcrDescription);
                interpretationDisclaimer = string.Format(DisclaimerTemplate,
                    string.Format("Haemophilus influenzae, Serotyp {0}", serotypePcrDescription));
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