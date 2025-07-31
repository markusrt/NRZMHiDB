using FluentAssertions;
using HaemophilusWeb.Models;
using HaemophilusWeb.Models.Meningo;
using NUnit.Framework;

namespace HaemophilusWeb.Domain
{
    public class IsolateInterpretationTests
    {
        private readonly IsolateInterpretation isolateInterpretation = new IsolateInterpretation();

        [Test]
        public void Interpret_EmptyIsolate_IsUndetermined()
        {
            var isolate = new Isolate();

            var interpretation = isolateInterpretation.Interpret(isolate);

            interpretation.Interpretation.Should().Contain("Diskrepante Ergebnisse");
            interpretation.InterpretationDisclaimer.Should().BeEmpty();
        }

        [Test]
        [TestCase(SerotypeAgg.A, SerotypePcr.A)]
        [TestCase(SerotypeAgg.B, SerotypePcr.B)]
        [TestCase(SerotypeAgg.C, SerotypePcr.C)]
        [TestCase(SerotypeAgg.D, SerotypePcr.D)]
        [TestCase(SerotypeAgg.E, SerotypePcr.E)]
        [TestCase(SerotypeAgg.F, SerotypePcr.F)]
        [TestCase(SerotypeAgg.A, SerotypePcr.NotDetermined)]
        [TestCase(SerotypeAgg.B, SerotypePcr.NotDetermined)]
        [TestCase(SerotypeAgg.C, SerotypePcr.NotDetermined)]
        [TestCase(SerotypeAgg.D, SerotypePcr.NotDetermined)]
        [TestCase(SerotypeAgg.E, SerotypePcr.NotDetermined)]
        [TestCase(SerotypeAgg.F, SerotypePcr.NotDetermined)]
        public void
            Interpret_DistinctAgglutinationWithPositiveBexA_EitherMatchingOrNotDeterminedSerotype_IsSpecificTyping(
            SerotypeAgg serotypeAgg,
            SerotypePcr serotypePcr)
        {
            var serotypeString = serotypeAgg.ToString().ToLower();
            var isolate = new Isolate
            {
                SerotypePcr = serotypePcr,
                Agglutination = serotypeAgg,
                BexA = TestResult.Positive,
                Sending = new Sending {SamplingLocation = SamplingLocation.Blood}
            };

            var interpretation = isolateInterpretation.Interpret(isolate);

            var expectedInterpretation = string.Format("Die Ergebnisse sprechen für eine Infektion mit Haemophilus influenzae des Serotyp {0} (Hi{0}).",
                serotypeString);
            interpretation.Interpretation.Should().Be(expectedInterpretation);
            interpretation.InterpretationPreliminary.Should().Contain("Diskrepante");
            interpretation.InterpretationDisclaimer.Should().Contain($"Meldekategorie dieses Befundes: Haemophilus influenzae, Serotyp {serotypeString}");
            interpretation.InterpretationDisclaimer.Should().Contain("Der direkte Nachweis von Haemophilus influenzae aus Blut oder Liquor ist nach §7 IfSG meldepflichtig.");
        }

        [Test]
        [TestCase(SerotypeAgg.A, SerotypePcr.NotDetermined)]
        [TestCase(SerotypeAgg.B, SerotypePcr.NotDetermined)]
        [TestCase(SerotypeAgg.C, SerotypePcr.NotDetermined)]
        [TestCase(SerotypeAgg.D, SerotypePcr.NotDetermined)]
        [TestCase(SerotypeAgg.E, SerotypePcr.NotDetermined)]
        [TestCase(SerotypeAgg.F, SerotypePcr.NotDetermined)]
        public void
            Interpret_DistinctAgglutinationWithNotDeterminedBexA_NotDeterminedSerotype_IsSpecificPreliminaryTyping(
                SerotypeAgg serotypeAgg,
                SerotypePcr serotypePcr)
        {
            var serotypeString = serotypeAgg.ToString().ToLower();
            var isolate = new Isolate
            {
                SerotypePcr = serotypePcr,
                Agglutination = serotypeAgg,
                BexA = TestResult.NotDetermined,
                Sending = new Sending {SamplingLocation = SamplingLocation.Blood}
            };

            var interpretation = isolateInterpretation.Interpret(isolate);

            var expectedInterpretationPreliminary = string.Format("Das Ergebnis spricht für eine Infektion mit Haemophilus influenzae des Serotyp {0} (Hi{0}).",
                serotypeString);
            interpretation.Interpretation.Should().Contain("Diskrepante");
            interpretation.InterpretationPreliminary.Should().Be(expectedInterpretationPreliminary);
            interpretation.InterpretationDisclaimer.Should().Contain($"Meldekategorie dieses Befundes: Haemophilus influenzae, Serotyp {serotypeString}");
            interpretation.InterpretationDisclaimer.Should().Contain("Der direkte Nachweis von Haemophilus influenzae aus Blut oder Liquor ist nach §7 IfSG meldepflichtig.");
        }

        [Test]
        public void
            Interpret_DistinctAgglutinationWithNotDeterminedBexA_NotDeterminedSerotype_NonInvasive_IsSpecificPreliminaryTypingWithEmptyDisclaimer()
        {
            var serotypeString = SerotypeAgg.E.ToString().ToLower();
            var isolate = new Isolate
            {
                SerotypePcr = SerotypePcr.NotDetermined,
                Agglutination = SerotypeAgg.E,
                BexA = TestResult.NotDetermined,
                Sending = new Sending {SamplingLocation = SamplingLocation.OtherNonInvasive}
            };

            var interpretation = isolateInterpretation.Interpret(isolate);

            var expectedInterpretationPreliminary = string.Format("Das Ergebnis spricht für eine Infektion mit Haemophilus influenzae des Serotyp {0} (Hi{0}).",
                serotypeString);
            interpretation.Interpretation.Should().Contain("Diskrepante");
            interpretation.InterpretationPreliminary.Should().Be(expectedInterpretationPreliminary);
            interpretation.InterpretationDisclaimer.Should().Be("Eine molekularbiologische Typisierung und Resistenztestungen werden bei nicht-invasiven Isolaten aus epidemiologischen und Kostengründen nicht durchgeführt.");
            interpretation.InterpretationDisclaimer.Should().NotContain("Der direkte Nachweis von Haemophilus influenzae aus Blut oder Liquor ist nach §7 IfSG meldepflichtig.");
        }

        [Test]
        [TestCase(SerotypePcr.A)]
        [TestCase(SerotypePcr.B)]
        [TestCase(SerotypePcr.C)]
        [TestCase(SerotypePcr.D)]
        [TestCase(SerotypePcr.E)]
        [TestCase(SerotypePcr.F)]
        public void Interpret_NegativeAgglutinationButDistinctSerotypeWithNegativeBexA_IsSpecificTyping(
            SerotypePcr serotypePcr)
        {
            var serotypeString = serotypePcr.ToString().ToLower();
            var isolate = new Isolate
            {
                SerotypePcr = serotypePcr,
                Agglutination = SerotypeAgg.Negative,
                BexA = TestResult.Negative,
                Sending = new Sending {SamplingLocation = SamplingLocation.Blood}
            };

            var interpretation = isolateInterpretation.Interpret(isolate);
            var expectedInterpretation = $"Die Ergebnisse sprechen für einen phänotpischen nicht-typisierbaren Haemophilus influenzae (NTHi). Ein vorhandener genetischer Kapsellocus für Polysaccharide des Serotyps {serotypeString} wird nicht exprimiert.";
            interpretation.Interpretation.Should().Be(expectedInterpretation);
            interpretation.InterpretationPreliminary.Should().Contain("Diskrepante");
            interpretation.InterpretationDisclaimer.Should().Contain("Meldekategorie dieses Befundes: Haemophilus influenzae, unbekapselt.");
            interpretation.InterpretationDisclaimer.Should().Contain("Der direkte Nachweis von Haemophilus influenzae aus Blut oder Liquor ist nach §7 IfSG meldepflichtig.");
        }

        [Test]
        public void Interpret_OnlyNegativeAgglutination_TypingNotEfficient()
        {
            var isolate = new Isolate
            {
                SerotypePcr = SerotypePcr.NotDetermined,
                Agglutination = SerotypeAgg.Negative,
                BexA = TestResult.NotDetermined,
                Sending = new Sending {SamplingLocation = SamplingLocation.Blood}
            };

            var interpretation = isolateInterpretation.Interpret(isolate);

            var expectedInterpretation = "Das Ergebnis spricht für einen unbekapselten Haemophilus influenzae (sog. \"nicht-typisierbarer\" H. influenzae, NTHi).";
            interpretation.Interpretation.Should().Be(expectedInterpretation + " Eine molekularbiologische Typisierung wurde aus epidemiologischen und Kostengründen nicht durchgeführt.");
            interpretation.InterpretationPreliminary.Should().Be(expectedInterpretation);
        }

        [Test]
        [TestCase(SerotypePcr.Negative)]
        [TestCase(SerotypePcr.NotDetermined)]
        public void Interpret_BexaAndAgglutinationIsNegative_WithRespectiveSeroType_NotTypable(SerotypePcr serotype)
        {
            var isolate = new Isolate
            {
                SerotypePcr = serotype,
                Agglutination = SerotypeAgg.Negative,
                BexA = TestResult.Negative,
                Sending = new Sending {SamplingLocation = SamplingLocation.Blood}
            };

            var interpretation = isolateInterpretation.Interpret(isolate);

            interpretation.Interpretation.Should().Contain("nicht-typisierbar");
            interpretation.InterpretationPreliminary.Should().Contain("Diskrepante");
            interpretation.InterpretationDisclaimer.Should()
                .Contain("Meldekategorie dieses Befundes: Haemophilus influenzae, unbekapselt.");
            interpretation.InterpretationDisclaimer.Should().Contain("Der direkte Nachweis von Haemophilus influenzae aus Blut oder Liquor ist nach §7 IfSG meldepflichtig.");

        }

        [TestCase(SerotypePcr.Negative)]
        [TestCase(SerotypePcr.NotDetermined)]
        public void Interpret_BexaAndAgglutinationIsNegative_WithRespectiveSeroType_NonInvasive_NotTypable(SerotypePcr serotype)
        {
            var isolate = new Isolate
            {
                SerotypePcr = serotype,
                Agglutination = SerotypeAgg.Negative,
                BexA = TestResult.Negative,
                Sending = new Sending {SamplingLocation = SamplingLocation.OtherNonInvasive}
            };

            var interpretation = isolateInterpretation.Interpret(isolate);

            interpretation.Interpretation.Should().Contain("nicht-typisierbar");
            interpretation.InterpretationPreliminary.Should().Contain("Diskrepante");
            interpretation.InterpretationDisclaimer.Should().Be("Eine molekularbiologische Typisierung und Resistenztestungen werden bei nicht-invasiven Isolaten aus epidemiologischen und Kostengründen nicht durchgeführt.");

        }

        [Test]
        public void Interpret_AgglutinationBexAAndSerotypAreNotDeterminedButGrowthIsYes_WithRespectiveSeroType_RequestForResend()
        {
            var isolate = new Isolate
            {
                SerotypePcr = SerotypePcr.NotDetermined,
                Agglutination = SerotypeAgg.NotDetermined,
                BexA = TestResult.NotDetermined,
                Growth = YesNoOptional.Yes
            };

            var interpretation = isolateInterpretation.Interpret(isolate);

            interpretation.Interpretation.Should().Contain("Um Wiedereinsendung wird gebeten");
            interpretation.InterpretationPreliminary.Should().Contain("Diskrepante");
            interpretation.InterpretationDisclaimer.Should()
                .Contain("Eine telefonische Vorabmitteilung ist erfolgt.");
        }

        [Test]
        public void Interpret_AgglutinationBexAAndSerotypAreNotDetermined_ButGrowthIsNo_ReportEvaluation()
        {
            var isolate = new Isolate
            {
                SerotypePcr = SerotypePcr.NotDetermined,
                Agglutination = SerotypeAgg.NotDetermined,
                BexA = TestResult.NotDetermined,
                Growth = YesNoOptional.No,
                Evaluation = Evaluation.HaemophilusSpeciesNoHaemophilusInfluenzae
            };

            var interpretation = isolateInterpretation.Interpret(isolate);

            interpretation.Interpretation.Should().Contain("Kein Nachweis von Haemophilus influenzae.");
            interpretation.InterpretationPreliminary.Should().Contain("Diskrepante");
            interpretation.InterpretationDisclaimer.Should()
                .Contain("Beim eingesendeten Isolat handelt es sich am ehesten um Haemophilus sp., nicht H. influenzae.");
        }

        [Test]
        public void Interpret_MatchingHaemoStemRule_UsesRuleBasedInterpretation()
        {
            var isolate = new Isolate
            {
                SerotypePcr = SerotypePcr.B,
                Agglutination = SerotypeAgg.B,
                BexA = TestResult.Positive,
                Oxidase = TestResult.Negative,
                Sending = new Sending { SamplingLocation = SamplingLocation.Blood }
            };

            var interpretation = isolateInterpretation.Interpret(isolate);

            interpretation.Report.Should().NotBeNull();
            interpretation.Report.Should().Contain(s => s.Contains("Haemophilus influenzae des Serotyp b"));
            isolateInterpretation.Rule.Should().Be("HaemoStemInterpretation_01");
            isolateInterpretation.Typings.Should().NotBeEmpty();
        }

        [Test]
        public void Interpret_MatchingHaemoNativeMaterialRule_UsesRuleBasedInterpretation()
        {
            var isolate = new Isolate
            {
                RibosomalRna16S = NativeMaterialTestResult.Positive,
                RibosomalRna16SBestMatch = "Haemophilus influenzae",
                RealTimePcr = NativeMaterialTestResult.Positive,
                RealTimePcrResult = RealTimePcrResult.HaemophilusInfluenzae,
                Sending = new Sending 
                { 
                    SamplingLocation = SamplingLocation.Blood,
                    Material = Material.NativeMaterial
                }
            };

            var interpretation = isolateInterpretation.Interpret(isolate);

            interpretation.Report.Should().NotBeNull();
            interpretation.Report.Should().Contain(s => s.Contains("Haemophilus influenzae-spezifische DNA"));
            isolateInterpretation.Rule.Should().Be("HaemoNativeMaterialInterpretation_01");
            isolateInterpretation.Typings.Should().NotBeEmpty();
        }

        [Test]
        public void Interpret_NoMatchingRule_FallsBackToOriginalLogic()
        {
            var isolate = new Isolate
            {
                SerotypePcr = SerotypePcr.A, // Different from rule which expects B
                Agglutination = SerotypeAgg.A,
                BexA = TestResult.Positive,
                Oxidase = TestResult.Positive, // Different from rule which expects Negative
                Sending = new Sending { SamplingLocation = SamplingLocation.Blood }
            };

            var interpretation = isolateInterpretation.Interpret(isolate);

            // Should fall back to original hardcoded interpretation logic
            interpretation.Interpretation.Should().Contain("Haemophilus influenzae des Serotyp a");
            isolateInterpretation.Rule.Should().BeNull(); // No rule matched
        }

    }
}