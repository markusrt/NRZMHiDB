using HaemophilusWeb.Models;

namespace HaemophilusWeb.Domain
{
    public static class IsolateInterpretation
    {
        public static InterpretationResult Interpret(Isolate isolate)
        {
            if (isolate.Agglutination == Serotype.Negative)
            {
                if (isolate.BexA == TestResult.Negative)
                {
                    if (isolate.SerotypePcr == Serotype.Negative)
                    {

                        return new InterpretationResult
                        {
                            Interpretation =
                                "Die Ergebnisse sprechen für einen nicht-typisierbaren Haemophilus influenzae (NTHi).",
                            InterpretationDisclaimer =
                                "Der Nachweis von Haemophilus influenzae aus primär sterilem Material ist nach §7 IfSG meldepflichtig. Meldekategorie dieses Befundes: Haemophilus influenzae, unbekapselt."
                        };
                    }
                }
            }
            return new InterpretationResult
            {
                Interpretation = "Abc",
                InterpretationDisclaimer = "Def"
            };
        }
    }

    public class InterpretationResult
    {
        public string Interpretation { get; set; }
        public string InterpretationDisclaimer { get; set; }
    }
}