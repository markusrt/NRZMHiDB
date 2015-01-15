using FluentAssertions;
using HaemophilusWeb.Models;
using NUnit.Framework;

namespace HaemophilusWeb.Domain
{
    public class IsolateInterpretationTests
    {
        [Test]
        public void Interpret_EmptyIsolate_IsUndetermined()
        {
            var isolate = new IsolateBase();

            var interpretation = IsolateInterpretation.Interpret(isolate);

            interpretation.Interpretation.Should().Contain("Diskrepante Ergebnisse");
            interpretation.InterpretationDisclaimer.Should().BeEmpty();
        }

        [Test]
        [TestCase(SerotypeAgg.A, SerotypePcr.A)]
        [TestCase(SerotypeAgg.A, SerotypePcr.NotDetermined)]
        [TestCase(SerotypeAgg.B, SerotypePcr.B)]
        [TestCase(SerotypeAgg.B, SerotypePcr.NotDetermined)]
        [TestCase(SerotypeAgg.C, SerotypePcr.C)]
        [TestCase(SerotypeAgg.C, SerotypePcr.NotDetermined)]
        [TestCase(SerotypeAgg.D, SerotypePcr.D)]
        [TestCase(SerotypeAgg.D, SerotypePcr.NotDetermined)]
        [TestCase(SerotypeAgg.E, SerotypePcr.E)]
        [TestCase(SerotypeAgg.E, SerotypePcr.NotDetermined)]
        [TestCase(SerotypeAgg.F, SerotypePcr.F)]
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

            var interpretation = IsolateInterpretation.Interpret(isolate);

            var expectedInterpretation = string.Format("Infektion mit Haemophilus influenzae des Serotyp {0} (Hi{0}).",
                serotypeString);
            var expectedDisclaimer = string.Format(
                "Meldekategorie dieses Befundes: Haemophilus influenzae, Serotyp {0}", serotypeString);
            interpretation.Interpretation.Should().Contain(expectedInterpretation);
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

            var interpretation = IsolateInterpretation.Interpret(isolate);

            var expectedInterpretation = string.Format("Kapsellocus für Polysaccharide des Serotyps {0}", serotypeString);
            interpretation.Interpretation.Should().Contain(expectedInterpretation);
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

            var interpretation = IsolateInterpretation.Interpret(isolate);

            interpretation.Interpretation.Should().Contain("aus epidemiologischen und Kostengründen nicht durchgeführt");
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

            var interpretation = IsolateInterpretation.Interpret(isolate);

            interpretation.Interpretation.Should().Contain("nicht-typisierbar");
            interpretation.InterpretationDisclaimer.Should()
                .Contain("Meldekategorie dieses Befundes: Haemophilus influenzae, unbekapselt.");
        }
    }
}