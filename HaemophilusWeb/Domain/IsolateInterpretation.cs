﻿using System;
using System.Collections.Generic;
using HaemophilusWeb.Models;
using HaemophilusWeb.Views.Utils;

namespace HaemophilusWeb.Domain
{
    public class IsolateInterpretation
    {
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
                Interpretation = interpretation,
                InterpretationPreliminary = interpretationPreliminary,
                InterpretationDisclaimer = interpretationDisclaimer
            };
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

        public string Comment { get; set; }
    }
}