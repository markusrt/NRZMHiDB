using FluentAssertions;
using HaemophilusWeb.Models;
using NUnit.Framework;

namespace HaemophilusWeb.Domain
{
    public class IsolateInterpretationTests
    {
        private readonly IsolateInterpretation isolateInterpretation = new IsolateInterpretation();

        [Test]
        public void Interpret_EmptyIsolate_IsUndetermined()
        {
            var isolate = new IsolateBase();

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
            var isolate = new IsolateBase
            {
                SerotypePcr = serotypePcr,
                Agglutination = serotypeAgg,
                BexA = TestResult.Positive
            };

            var interpretation = isolateInterpretation.Interpret(isolate);

            var expectedInterpretation = string.Format("Die Ergebnisse sprechen für eine Infektion mit Haemophilus influenzae des Serotyp {0} (Hi{0}).",
                serotypeString);
            var expectedDisclaimer = $"Meldekategorie dieses Befundes: Haemophilus influenzae, Serotyp {serotypeString}";
            interpretation.Interpretation.Should().Be(expectedInterpretation);
            interpretation.InterpretationPreliminary.Should().Contain("Diskrepante");
            interpretation.InterpretationDisclaimer.Should().Contain(expectedDisclaimer);
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
            var isolate = new IsolateBase
            {
                SerotypePcr = serotypePcr,
                Agglutination = serotypeAgg,
                BexA = TestResult.NotDetermined
            };

            var interpretation = isolateInterpretation.Interpret(isolate);

            var expectedInterpretationPreliminary = string.Format("Das Ergebnis spricht für eine Infektion mit Haemophilus influenzae des Serotyp {0} (Hi{0}).",
                serotypeString);
            var expectedDisclaimer =
                $"Meldekategorie dieses Befundes: Haemophilus influenzae, Serotyp {serotypeString}";
            interpretation.Interpretation.Should().Contain("Diskrepante");
            interpretation.InterpretationPreliminary.Should().Be(expectedInterpretationPreliminary);
            interpretation.InterpretationDisclaimer.Should().Contain(expectedDisclaimer);
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
            var isolate = new IsolateBase
            {
                SerotypePcr = serotypePcr,
                Agglutination = SerotypeAgg.Negative,
                BexA = TestResult.Negative
            };

            var interpretation = isolateInterpretation.Interpret(isolate);
            var expectedInterpretation = $"Die Ergebnisse sprechen für einen phänotpischen nicht-typisierbaren Haemophilus influenzae (NTHi). Ein vorhandener genetischer Kapsellocus für Polysaccharide des Serotyps {serotypeString} wird nicht exprimiert.";
            interpretation.Interpretation.Should().Be(expectedInterpretation);
            interpretation.InterpretationPreliminary.Should().Contain("Diskrepante");
            interpretation.InterpretationDisclaimer.Should()
                .Contain("Meldekategorie dieses Befundes: Haemophilus influenzae, unbekapselt.");
        }

        [Test]
        public void Interpret_OnlyNegativeAgglutination_TypingNotEfficient()
        {
            var isolate = new IsolateBase
            {
                SerotypePcr = SerotypePcr.NotDetermined,
                Agglutination = SerotypeAgg.Negative,
                BexA = TestResult.NotDetermined
            };

            var interpretation = isolateInterpretation.Interpret(isolate);

            var expectedInterpretation = "Das Ergebnis spricht für einen nicht-typisierbaren Haemophilus influenzae (NTHi).";
            interpretation.Interpretation.Should().Be(expectedInterpretation + " Eine molekularbiologische Typisierung wurde aus epidemiologischen und Kostengründen nicht durchgeführt.");
            interpretation.InterpretationPreliminary.Should().Be(expectedInterpretation);
        }

        [Test]
        [TestCase(SerotypePcr.Negative)]
        [TestCase(SerotypePcr.NotDetermined)]
        public void Interpret_BexaAndAgglutinationIsNegative_WithRespectiveSeroType_NotTypable(SerotypePcr serotype)
        {
            var isolate = new IsolateBase
            {
                SerotypePcr = serotype,
                Agglutination = SerotypeAgg.Negative,
                BexA = TestResult.Negative
            };

            var interpretation = isolateInterpretation.Interpret(isolate);

            interpretation.Interpretation.Should().Contain("nicht-typisierbar");
            interpretation.InterpretationPreliminary.Should().Contain("Diskrepante");
            interpretation.InterpretationDisclaimer.Should()
                .Contain("Meldekategorie dieses Befundes: Haemophilus influenzae, unbekapselt.");

        }
    }
}